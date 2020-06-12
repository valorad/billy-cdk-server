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

    public async Task<CUDMessage> AddGame(string player, List<string> games)
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

    # region AddGame Aliases

    public async Task<CUDMessage> AddGame(Player player, string game)
    {
      return await AddGame(player.DBName, game);
    }

    public async Task<CUDMessage> AddGame(Player player, List<string> games)
    {
      return await AddGame(player.DBName, games);
    }

    public async Task<CUDMessage> AddGame(Player player, Game game)
    {
      return await AddGame(player.DBName, game.DBName);
    }

    public async Task<CUDMessage> AddGame(Player player, List<Game> games)
    {
      var gameNames = new List<string>();
      foreach (var game in games)
      {
        gameNames.Add(game.DBName);
      }
      return await AddGame(player.DBName, gameNames);
    }

    # endregion

  }

}