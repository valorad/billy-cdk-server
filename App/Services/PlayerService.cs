using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public class PlayerService : BaseDataService<Player>, IPlayerService
  {
    public PlayerService(IDBCollection collection) : base(collection.Players)
    {
      this.uniqueFieldName = "dbname";
    }

    public async Task<CUDMessage> AddGame(string player, string game)
    {
      string updateToken = "{"
        + " \"$push\": " + "{"
          + $"\"games\": \"{game}\""
          + "}"
        + "}";

      return await Update(player, JsonDocument.Parse(updateToken).RootElement);
    }

    public async Task<CUDMessage> AddGames(string player, List<string> games)
    {

      var gamesQuoted = from ele in games select $"\"{ele}\"";

      string gamesInText = string.Join(", ", gamesQuoted);

      string updateToken = "{"
        + " \"$push\": " + "{"
          + "\"games\": " + "{"
            + " \"$each\": " + $"[{gamesInText}]"
            + "}"
          + "}"
        + "}";

      return await Update(player, JsonDocument.Parse(updateToken).RootElement);
    }

    public async Task<bool> OwnsTheGame(string playerName, string gameName)
    {
      Player player = await Get(playerName);

      if (player is null) {
        Console.WriteLine("Player does not exist");
        return false;
      }

      return player.Games.Exists(ele => ele == gameName);

    }



  }

}