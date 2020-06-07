using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Models;
using GraphQL;

namespace App.Controllers.Graphs
{

    public partial class Query
  {

    [GraphQLMetadata("player")]
    public async Task<Player> GetPlayer(string dbname)
    {
      Player player = await playerService.Get(dbname);
      return player; 
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
        new DBViewOption() {
          Includes = new List<string>() {"dbname"}
        }
      );
      if (playerInDB is {}) {
        return new CUDMessage() {
          OK = false,
          NumAffected = 0,
          Message = $"Forbidden to add {newPlayer.DBName} becasue the same dbname already exists",
        };
      }

      if (newPlayer.Games is null) {
        newPlayer.Games = new List<string>() {};
      }

      CUDMessage message = await playerService.Add(newPlayer);
      if (!message.OK) {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newPlayer.DBName}. See log for more details.";
      } else {
        message.Message = $"Successfully added {newPlayer.DBName}.";
      }
      return message; 
    }

    [GraphQLMetadata("addPlayers")]
    public async Task<CUDMessage> AddPlayer(List<Player> newPlayers)
    {

      // stop here if list is empty
      if (newPlayers.Count <= 0) {
        return new CUDMessage() {
          OK = false,
          NumAffected = 0,
          Message = $"No players are added since the add list is empty.",
        };
      }

      // remove conflict players in the add list
      var dbnameQuoted = newPlayers.Select((ele) => $"\"{ele.DBName}\"" );
      string dbnameInText = string.Join(", ", dbnameQuoted);
      
      string conditions = "{"
      + " \"dbname\": " + "{"
        + "\"$in\": [" + dbnameInText + "]"
        + "}"
      + "}";
      List<Player> conflictPlayers = await playerService.Get(
        JsonDocument.Parse(conditions).RootElement,
        new DBViewOption() {
          Includes = new List<string>() {"dbname"}
        }
      );

      foreach (var player in conflictPlayers) {
        newPlayers.Remove(newPlayers.Find(ele => ele.DBName == player.DBName));
      }

      if (newPlayers.Count <= 0) {
        return new CUDMessage() {
          OK = false,
          NumAffected = 0,
          Message = $"Forbidden to add these players becasue they all have conflicted dbname-s with existing players.",
        };
      }

      // polyfill game field
      foreach (var player in newPlayers) {
        if (player.Games is null) {
          player.Games = new List<string>() {};
        }
      }

      CUDMessage message = await playerService.Add(newPlayers);
      if (!message.OK) {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newPlayers.Count} players. See logs for more details.";
      } else {
        message.Message = $"Successfully added {newPlayers.Count} players.";
      }
      return message; 
    }

  }


}