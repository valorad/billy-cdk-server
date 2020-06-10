using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Lib;
using App.Models;
using GraphQL;

namespace App.Controllers.Graphs
{
  public static class PlayerGraph
  {

    public static List<string> requiredFields = new List<string>() {
      "dbname"
    };

    public static bool HasRequiredFields(Player newPlayer)
    {
      // Normally we just need to check string and nullable fields. For non-nullable types,
      // GraphQL will stop the request for us already if the request mutation is invalid
      foreach (var field in requiredFields)
      {
        var value = Property.GetValue(newPlayer, field);
        if (value is null || string.IsNullOrWhiteSpace((string)value))
        {
          return false;
        }
      }

      return true;

    }

    public static void Polyfill(Player newPlayer)
    {

      if (newPlayer.Games is null)
      {
        newPlayer.Games = new List<string>() { };
      }

    }

    public static void Polyfill(List<Player> newPlayers)
    {

      foreach (var player in newPlayers)
      {
        PlayerGraph.Polyfill(player);
      }

    }

  }

  public partial class Query
  {

    [GraphQLMetadata("player")]
    public async Task<Player> GetPlayer(string dbname, string options)
    {

      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
        return await playerService.Get(dbname, viewOptions);
      }
      else
      {
        return await playerService.Get(dbname);
      }

    }

    [GraphQLMetadata("players")]
    public async Task<List<Player>> GetPlayers(string condition, string options)
    {

      JsonElement conditionInJson;

      if (string.IsNullOrWhiteSpace(condition))
      {
        conditionInJson = JsonDocument.Parse("{}").RootElement;
      }
      else
      {
        conditionInJson = JsonDocument.Parse(condition).RootElement;
      }

      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
        return await playerService.Get(conditionInJson, viewOptions);
      }
      else
      {
        return await playerService.Get(conditionInJson);
      }

    }

  }

  public partial class Mutation
  {

    [GraphQLMetadata("addPlayer")]
    public async Task<CUDMessage> AddPlayer(Player newPlayer)
    {

      // Check if has existing player
      Player playerInDB = await playerService.Get(
        newPlayer.DBName,
        new DBViewOption()
        {
          Includes = new List<string>() { "dbname" }
        }
      );
      if (playerInDB is { })
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Forbidden to add {newPlayer.DBName} because the same dbname already exists",
        };
      }

      // Check if necessary fields are provided
      bool hasRequiredFields = PlayerGraph.HasRequiredFields(newPlayer);
      if (!hasRequiredFields)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to add player because at least one required field is not provided. The required fields are: {string.Join(", ", PlayerGraph.requiredFields)}",
        };
      }

      // Polyfill the less important fields
      PlayerGraph.Polyfill(newPlayer);

      CUDMessage message = await playerService.Add(newPlayer);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newPlayer.DBName}. See log for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newPlayer.DBName}.";
      }
      return message;
    }

    [GraphQLMetadata("addPlayers")]
    public async Task<CUDMessage> AddPlayer(List<Player> newPlayers)
    {

      // stop here if list is empty
      if (newPlayers.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No players are added since the add list is empty.",
        };
      }

      // remove conflict players in the add list
      var dbnameQuoted = newPlayers.Select((ele) => $"\"{ele.DBName}\"");
      string dbnameInText = string.Join(", ", dbnameQuoted);

      string conditions = "{"
      + " \"dbname\": " + "{"
        + "\"$in\": [" + dbnameInText + "]"
        + "}"
      + "}";
      List<Player> conflictPlayers = await playerService.Get(
        JsonDocument.Parse(conditions).RootElement,
        new DBViewOption()
        {
          Includes = new List<string>() { "dbname" }
        }
      );

      foreach (var player in conflictPlayers)
      {
        newPlayers.Remove(newPlayers.Find(ele => ele.DBName == player.DBName));
      }

      // Check if necessary fields are provided
      var incompletePlayers = new List<Player>() { };
      foreach (var player in newPlayers)
      {
        if (!PlayerGraph.HasRequiredFields(player))
        {
          incompletePlayers.Add(player);
        }
      }
      // Remove incomplete players
      foreach (var player in incompletePlayers)
      {
        newPlayers.Remove(newPlayers.Find(ele => ele.DBName == player.DBName));
      }

      if (newPlayers.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No players to add because of non of these players are eligible",
        };
      }

      // polyfill
      PlayerGraph.Polyfill(newPlayers);

      CUDMessage message = await playerService.Add(newPlayers);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newPlayers.Count} players. See logs for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newPlayers.Count} players.";
      }
      return message;
    }

    [GraphQLMetadata("updatePlayer")]
    public async Task<CUDMessage> UpdatePlayer(string dbname, string token)
    {
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await playerService.Update(dbname, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update {dbname}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated {dbname}.";
      }

      return message;

    }

    [GraphQLMetadata("updatePlayers")]
    public async Task<CUDMessage> UpdatePlayers(string condition, string token)
    {

      if (string.IsNullOrWhiteSpace(condition) || condition.Equals("{}"))
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = "Condition cannot be empty or \"{}\" ",
        };
      }

      JsonElement conditionInJson = JsonDocument.Parse(condition).RootElement;
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await playerService.Update(conditionInJson, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated selected players.";
      }

      return message;

    }

    [GraphQLMetadata("deletePlayer")]
    public async Task<CUDMessage> DeletePlayer(string dbname)
    {

      CUDMessage message = await playerService.Delete(dbname);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete {dbname}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted {dbname}.";
      }

      return message;
    }

    [GraphQLMetadata("deletePlayers")]
    public async Task<CUDMessage> DeletePlayers(string condition)
    {

      if (string.IsNullOrWhiteSpace(condition) || condition.Equals("{}"))
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = "Condition cannot be empty or \"{}\" ",
        };
      }

      JsonElement conditionInJson = JsonDocument.Parse(condition).RootElement;

      CUDMessage message = await playerService.Delete(conditionInJson);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted selected players.";
      }

      return message;
    }

  }


}