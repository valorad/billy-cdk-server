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

      return await UpdateSingle(player, JsonDocument.Parse(updateToken).RootElement);
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

      return await UpdateSingle(player, JsonDocument.Parse(updateToken).RootElement);
    }
  }

}