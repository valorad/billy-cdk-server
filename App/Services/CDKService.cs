using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public class CDKService : BaseDataService<CDKey>, ICDKService
  {
    public IPlayerService playerService { get; }

    public CDKService(
      IDBCollection collection,
      IPlayerService playerService
      ) : base(collection.CDKeys)
    {
      this.uniqueFieldName = "_id";
      this.playerService = playerService;
    }

    public async Task<CDKey> GetByValue(string value) {
      string condition = "{"
        + $"\"value\": \"{value}\" "
      + "}";
      List<CDKey> cdKeys = await Get(JsonDocument.Parse(condition).RootElement);
      if (cdKeys is {} && cdKeys.Count >= 1) {
        return cdKeys[0];
      }
      return null;
    }

    public async Task<List<CDKey>> GetByValue(List<string> values) {
      var valueQuoted = from ele in values select $"\"{ele}\"";
      string valueInText = string.Join(", ", valueQuoted);
      string conditions = "{"
      + " \"value\": " + "{"
        + "\"$in\": [" + valueInText + "]"
        + "}"
      + "}";
      List<CDKey> cdKeys = await Get(JsonDocument.Parse(conditions).RootElement);
      return cdKeys;
    }

    public async Task<CDKey> Activate(string playerName, string value)
    {

      CDKey cdkey = await GetByValue(value);
      if (cdkey is null) {
        throw new System.Exception($"Unable to find CDKey {value}.");
      }

      // check if cdk already activated + if player already owns the game
      if (cdkey.IsActivated) {
        throw new System.Exception($"cdkey {value} has already been activated by {cdkey.Player}.");
      }

      Player player = await playerService.Get(playerName);
      bool IsTheGameOwned = player.Games.Exists(ele => ele == cdkey.Game);

      if (IsTheGameOwned) {
        throw new System.Exception($"{cdkey.Player} already owns {cdkey.Game}.");
      }

      // update CDKey isActivated field
      CUDMessage cdkUpdateResult = await UpdateIsActivated(value, true);

      if (!cdkUpdateResult.OK) {
        throw new System.Exception($"Failed to update CDKey {value}.");
      }

      if (cdkUpdateResult.NumAffected <= 0) {
        throw new System.Exception($"Invalid CDKey {value} provided.");
      }

      // mark the player who activates cdkey
      cdkUpdateResult = await UpdatePlayer(value, playerName);

      // add the game to player
      
      CUDMessage playerUpdateResult = await playerService.AddGame(playerName, cdkey.Game);

      if (!playerUpdateResult.OK) {
        throw new System.Exception($"Failed to add {cdkey.Game} to {playerName}.");
      }

      // return this cdkey
      cdkey = await Get(cdkey.ID);
      return cdkey;
    }

    public async Task<List<CDKey>> Activate(string playerName, List<string> values)
    {

      var validCDKeys = new List<CDKey>() {};
      foreach (var value in values) {
        CDKey validCDKey = await GetByValue(value);
        if (validCDKey is {}) {
          validCDKeys.Add(validCDKey);
        }
      }

      Player player = await playerService.Get(playerName);

      // check if cdk already activated + if player already owns the game
      validCDKeys = (
        from validCDKey in validCDKeys
        where (!validCDKey.IsActivated && !player.Games.Exists(ele => ele == validCDKey.Game))
        select validCDKey
      ).ToList();

      // update CDKey isActivated field
      // mark the player who activates cdkey
      var successfullyActivatedCDKeys = new List<CDKey>() {};

      foreach (var validCDKey in validCDKeys) {
        CUDMessage cdkUpdateResult = await UpdateIsActivated(validCDKey.Value, true);
        if (cdkUpdateResult.OK && (cdkUpdateResult.NumAffected > 0)) {
          cdkUpdateResult = await UpdatePlayer(validCDKey.Value, playerName);
          successfullyActivatedCDKeys.Add(validCDKey);
        }
      }

      // mark the player who activates cdkey
      foreach (var successfullyActivatedCDKey in successfullyActivatedCDKeys) {
        await UpdatePlayer(successfullyActivatedCDKey.Value, playerName);
      }

      // add the game to player
      var successfullyAddedCDKeys = new List<CDKey>() {};
      foreach (var cdKey in successfullyActivatedCDKeys) {
        CUDMessage playerUpdateResult = await playerService.AddGame(playerName, cdKey.Game);
        if (playerUpdateResult.OK) {
          successfullyAddedCDKeys.Add(cdKey);
        }
      }

      // return sucessfully updated cdkeys list
      var dbUpdatedCDkeys = new List<CDKey>() {};
      foreach (var cdKey in successfullyAddedCDKeys) {
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