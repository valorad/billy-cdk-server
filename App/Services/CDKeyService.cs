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

    public async Task<InstanceCUDMessage<CDKey>> Activate(string playerDBName, string value)
    {
      Player player = await playerService.Get(playerDBName);
      if (player is null)
      {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"Player {playerDBName} does not exist.",
          Instance = null,
        };
      }
      return await Activate(player, value);
    }

    public async Task<InstanceCUDMessage<CDKey>> Activate(string playerDBName, List<string> values)
    {
      Player player = await playerService.Get(playerDBName);
      if (player is null)
      {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"Player {playerDBName} does not exist.",
          Instance = null,
        };
      }
      return await Activate(player, values);
    }

    public async Task<InstanceCUDMessage<CDKey>> Activate(Player player, string value)
    {
      CDKey cdkey = await GetByValue(value);
      var activateMessage = new InstanceCUDMessage<CDKey>() {
        OK = false,
        NumAffected = 0,
        Instance = null,
      };

      if (cdkey is null)
      {
        activateMessage.Message = $"CDKey {value} does not exist.";
        return activateMessage;
      }

      // check if cdk already activated + if player already owns the game
      if (cdkey.IsActivated ?? false)
      {
        activateMessage.Message = $"CDKey {value} has already been activated by {cdkey.Player}.";
        return activateMessage;
      }

      // Player player = await playerService.Get(playerName);
      bool IsTheGameOwned = player.Games.Exists(ele => ele == cdkey.Game);

      if (IsTheGameOwned)
      {
        activateMessage.Message = $"{player.DBName} already owns {cdkey.Game}.";
        return activateMessage;
      }

      // update CDKey isActivated field
      CUDMessage cdkUpdateResult = await UpdateIsActivated(value, true);

      if (!cdkUpdateResult.OK)
      {
        Console.WriteLine(cdkUpdateResult.Message);
        activateMessage.Message = $"Failed to update CDKey {value}. See logs for details.";
        return activateMessage;
      }

      if (cdkUpdateResult.NumAffected <= 0)
      {
        activateMessage.Message = $"Failed to update CDKey {value}: Invalid CDKey {value} provided.";
        return activateMessage;
      }

      // mark the player who activates cdkey
      cdkUpdateResult = await UpdatePlayer(value, player.DBName);

      // add the game to player
      CUDMessage playerUpdateResult = await playerService.AddGame(player, cdkey.Game);
      if (!playerUpdateResult.OK)
      {
        activateMessage.Message = $"Failed to add {cdkey.Game} to {player.DBName}.";
        return activateMessage;
      }

      // return this cdkey

      cdkey = await Get(cdkey.ID);

      if (cdkey is null) {
        activateMessage.Message = $"Failed to retrieve CDKey {cdkey.ID}.";
        return activateMessage;
      }

      activateMessage.Instance = cdkey;
      activateMessage.Message = $"Successfully activated CDKey with Value = {cdkey.Value} for Player {player.DBName}.";

      return activateMessage;
    }

    public async Task<InstanceCUDMessage<CDKey>> Activate(Player player, List<string> values)
    {

      int input = values.Count;

      // check if cdkey list is empty
      if (input <= 0) {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"No CDKeys to activate since the input list is empty.",
          Instances = null,
        };
      }

      List<CDKey> validCDKeys = await GetByValue(values);

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
          cdkUpdateResult = await UpdatePlayer(validCDKey.Value, player.DBName);
          successfullyActivatedCDKeys.Add(validCDKey);
        }
      }

      // mark the player who activates cdkey
      foreach (var successfullyActivatedCDKey in successfullyActivatedCDKeys)
      {
        await UpdatePlayer(successfullyActivatedCDKey.Value, player.DBName);
      }

      // add the game to player
      var successfullyAddedCDKeys = new List<CDKey>() { };
      foreach (var cdKey in successfullyActivatedCDKeys)
      {
        CUDMessage playerUpdateResult = await playerService.AddGame(player.DBName, cdKey.Game);
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

      if (dbUpdatedCDkeys.Count <= 0) {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"No CDKeys to activate because non of these inputs are valid. Try activate them one by one if you are not sure why.",
          Instances = null,
        };
      }

      return new InstanceCUDMessage<CDKey>() {
        OK = true,
        NumAffected = dbUpdatedCDkeys.Count,
        Message = $"Activate CDKeys for Player with dbname = {player.DBName}: {dbUpdatedCDkeys.Count} success, {input - dbUpdatedCDkeys.Count} failure.",
        Instances = dbUpdatedCDkeys,
      };
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