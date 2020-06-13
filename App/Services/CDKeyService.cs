using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public class CDKeyService : BaseDataService<CDKey>, ICDKeyService
  {
    public IPlayerService playerService { get; }

    public CDKeyService(
      IDBCollection collection,
      IPlayerService playerService
      ) : base(collection.CDKeys)
    {
      this.uniqueFieldName = "_id";
      this.playerService = playerService;
    }

    public async Task<CDKey> GetByValue(string value, IDBViewOption options = null)
    {
      string condition = "{"
        + $"\"value\": \"{value}\" "
      + "}";

      List<CDKey> cdKeys;

      if (options is { })
      {
        cdKeys = await Get(JsonDocument.Parse(condition).RootElement, options);
      }
      else
      {
        cdKeys = await Get(JsonDocument.Parse(condition).RootElement);
      }

      return cdKeys.ElementAtOrDefault(0);

    }

    public async Task<List<CDKey>> GetByValue(List<string> values, IDBViewOption options = null)
    {
      var valueQuoted = from ele in values select $"\"{ele}\"";
      string valueInText = string.Join(", ", valueQuoted);
      string condition = "{"
      + " \"value\": " + "{"
        + "\"$in\": [" + valueInText + "]"
        + "}"
      + "}";

      List<CDKey> cdKeys;

      if (options is { })
      {
        cdKeys = await Get(JsonDocument.Parse(condition).RootElement, options);
      }
      else
      {
        cdKeys = await Get(JsonDocument.Parse(condition).RootElement);
      }

      return cdKeys;
    }

    public async Task<CDKey> Activate(string playerDBName, string value)
    {
      Player player = await playerService.Get(playerDBName);
      if (player is null)
      {
        throw new Exception($"Player {playerDBName} does not exist.");
      }
      return await Activate(player, value);
    }

    public async Task<CDKey> Activate(Player player, string value)
    {
      CDKey cdkey = await GetByValue(value);
      if (cdkey is null)
      {
        throw new Exception($"Unable to find CDKey {value}.");
      }

      // check if cdk already activated + if player already owns the game
      if (cdkey.IsActivated ?? false)
      {
        throw new Exception($"cdkey {value} has already been activated by {cdkey.Player}.");
      }

      // Player player = await playerService.Get(playerName);
      bool IsTheGameOwned = player.Games.Exists(ele => ele == cdkey.Game);

      if (IsTheGameOwned)
      {
        throw new Exception($"{player.DBName} already owns {cdkey.Game}.");
      }

      // update CDKey isActivated field
      CUDMessage cdkUpdateResult = await UpdateIsActivated(value, true);

      if (!cdkUpdateResult.OK)
      {
        throw new Exception($"Failed to update CDKey {value}.");
      }

      if (cdkUpdateResult.NumAffected <= 0)
      {
        throw new Exception($"Invalid CDKey {value} provided.");
      }

      // mark the player who activates cdkey
      cdkUpdateResult = await UpdatePlayer(value, player.DBName);

      // add the game to player
      CUDMessage playerUpdateResult = await playerService.AddGame(player, cdkey.Game);
      if (!playerUpdateResult.OK)
      {
        throw new Exception($"Failed to add {cdkey.Game} to {player.DBName}.");
      }

      // return this cdkey
      cdkey = await Get(cdkey.ID);
      return cdkey;
    }

    public async Task<List<CDKey>> Activate(string playerName, List<string> values)
    {

      List<CDKey> validCDKeys = await GetByValue(values);

      Player player = await playerService.Get(playerName);

      // check if cdk already activated + if player already owns the game
      validCDKeys = (
        from validCDKey in validCDKeys
        where (!(validCDKey.IsActivated ?? false) && !player.Games.Exists(ele => ele == validCDKey.Game))
        select validCDKey
      ).ToList();

      // update CDKey isActivated field
      // mark the player who activates cdkey
      var successfullyActivatedCDKeys = new List<CDKey>() { };

      foreach (var validCDKey in validCDKeys)
      {
        CUDMessage cdkUpdateResult = await UpdateIsActivated(validCDKey.Value, true);
        if (cdkUpdateResult.OK && (cdkUpdateResult.NumAffected > 0))
        {
          cdkUpdateResult = await UpdatePlayer(validCDKey.Value, playerName);
          successfullyActivatedCDKeys.Add(validCDKey);
        }
      }

      // mark the player who activates cdkey
      foreach (var successfullyActivatedCDKey in successfullyActivatedCDKeys)
      {
        await UpdatePlayer(successfullyActivatedCDKey.Value, playerName);
      }

      // add the game to player
      var successfullyAddedCDKeys = new List<CDKey>() { };
      foreach (var cdKey in successfullyActivatedCDKeys)
      {
        CUDMessage playerUpdateResult = await playerService.AddGame(playerName, cdKey.Game);
        if (playerUpdateResult.OK)
        {
          successfullyAddedCDKeys.Add(cdKey);
        }
      }

      // return sucessfully updated cdkeys list
      var dbUpdatedCDkeys = new List<CDKey>() { };
      foreach (var cdKey in successfullyAddedCDKeys)
      {
        dbUpdatedCDkeys.Add(await Get(cdKey.ID));
      }
      return dbUpdatedCDkeys;
    }

    public async Task<CUDMessage> ActivateByDBID(string id)
    {
      string updateToken = "{"
        + "\"$set\": " + "{"
          + "\"isActivated\": true "
        + "}"
      + "}";
      return await Update(id, JsonDocument.Parse(updateToken).RootElement);
    }

    public async Task<CUDMessage> ActivateByDBID(List<string> ids)
    {
      var idFragments = from ele in ids select $"\"{ele}\"";
      string idFragmentsInText = string.Join(", ", idFragments);
      string conditions = "{"
      + " \"_id\": " + "{"
        + "\"$in\": [" + idFragmentsInText + "]"
        + "}"
      + "}";
      string updateToken = "{"
        + "\"$set\": " + "{"
          + "\"isActivated\": true"
        + "}"
      + "}";
      return await Update(
        JsonDocument.Parse(conditions).RootElement,
        JsonDocument.Parse(updateToken).RootElement
      );

    }

    public async Task<CUDMessage> UpdateIsActivated(string cdkValue, bool newStatus)
    {
      string condition = "{"
        + $"\"value\": \"{cdkValue}\" "
      + "}";

      string updateToken = "{"
        + "\"$set\": " + "{"
          + $"\"isActivated\": {newStatus.ToString().ToLower()} "
        + "}"
      + "}";

      return await Update(
        JsonDocument.Parse(condition).RootElement,
        JsonDocument.Parse(updateToken).RootElement
      );

    }

    public async Task<CUDMessage> UpdatePlayer(string cdkValue, string newPlayerName)
    {
      string condition = "{"
        + $"\"value\": \"{cdkValue}\""
      + "}";
      string updateToken = "{"
        + "\"$set\": " + "{"
          + $"\"player\": \"{newPlayerName}\" "
        + "}"
      + "}";

      return await Update(
        JsonDocument.Parse(condition).RootElement,
        JsonDocument.Parse(updateToken).RootElement
      );

    }
  }
}