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
  public static class GameGraph
  {
    public static List<string> requiredFields = new List<string>() {
      "dbname"
    };

    public static bool HasRequiredFields(Game newGame)
    {
      // Normally we just need to check string and nullable fields. For non-nullable types,
      // GraphQL will stop the request for us already if the request mutation is invalid
      foreach (var field in requiredFields)
      {
        var value = Property.GetValue(newGame, field);
        if (value is null || string.IsNullOrWhiteSpace((string)value))
        {
          return false;
        }
      }

      return true;

    }
    
  }


  public partial class Query
  {

    [GraphQLMetadata("game")]
    public async Task<Game> GetGame(string dbname, string options)
    {

      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
        return await gameService.Get(dbname, viewOptions);
      }
      else
      {
        return await gameService.Get(dbname);
      }

    }

    [GraphQLMetadata("games")]
    public async Task<List<Game>> GetGames(string condition, string options)
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
        return await gameService.Get(conditionInJson, viewOptions);
      }
      else
      {
        return await gameService.Get(conditionInJson);
      }

    }

  }

  public partial class Mutation
  {

    [GraphQLMetadata("addGame")]
    public async Task<CUDMessage> AddGame(Game newGame)
    {

      // Check if has existing game
      Game gameInDB = await gameService.Get(
        newGame.DBName,
        new DBViewOption()
        {
          Includes = new List<string>() { "dbname" }
        }
      );
      if (gameInDB is { })
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Forbidden to add {newGame.DBName} because the same dbname already exists",
        };
      }

      // Check if necessary fields are provided
      bool hasRequiredFields = GameGraph.HasRequiredFields(newGame);
      if (!hasRequiredFields)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to add game because at least one required field is not provided. The required fields are: {string.Join(", ", GameGraph.requiredFields)}",
        };
      }

      CUDMessage message = await gameService.Add(newGame);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newGame.DBName}. See log for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newGame.DBName}.";
      }
      return message;
    }

    [GraphQLMetadata("addGames")]
    public async Task<CUDMessage> AddGame(List<Game> newGames)
    {

      // stop here if list is empty
      if (newGames.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No games are added since the add list is empty.",
        };
      }

      // remove conflict games in the add list
      var dbnameQuoted = newGames.Select((ele) => $"\"{ele.DBName}\"");
      string dbnameInText = string.Join(", ", dbnameQuoted);

      string conditions = "{"
      + " \"dbname\": " + "{"
        + "\"$in\": [" + dbnameInText + "]"
        + "}"
      + "}";
      List<Game> conflictGames = await gameService.Get(
        JsonDocument.Parse(conditions).RootElement,
        new DBViewOption()
        {
          Includes = new List<string>() { "dbname" }
        }
      );

      foreach (var game in conflictGames)
      {
        newGames.Remove(newGames.Find(ele => ele.DBName == game.DBName));
      }

      // Check if necessary fields are provided
      var incompleteGames = new List<Game>() { };
      foreach (var game in newGames)
      {
        if (!GameGraph.HasRequiredFields(game))
        {
          incompleteGames.Add(game);
        }
      }
      // Remove incomplete games
      foreach (var game in incompleteGames)
      {
        newGames.Remove(newGames.Find(ele => ele.DBName == game.DBName));
      }

      if (newGames.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No games to add because of non of these games are eligible",
        };
      }

      CUDMessage message = await gameService.Add(newGames);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newGames.Count} games. See logs for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newGames.Count} games.";
      }
      return message;
    }

    [GraphQLMetadata("updateGame")]
    public async Task<CUDMessage> UpdateGame(string dbname, string token)
    {
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await gameService.Update(dbname, updateToken);

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

    [GraphQLMetadata("updateGames")]
    public async Task<CUDMessage> UpdateGames(string condition, string token)
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

      CUDMessage message = await gameService.Update(conditionInJson, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated selected games.";
      }

      return message;

    }

    [GraphQLMetadata("deleteGame")]
    public async Task<CUDMessage> DeleteGame(string dbname)
    {

      CUDMessage message = await gameService.Delete(dbname);
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

    [GraphQLMetadata("deleteGames")]
    public async Task<CUDMessage> DeleteGames(string condition)
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

      CUDMessage message = await gameService.Delete(conditionInJson);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted selected games.";
      }

      return message;
    }

  }



}