using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public interface IPlayerService
  {
    Task<Player> GetPlayer(string dbname);
    Task<IEnumerable<Player>> GetPlayerList(JsonElement condition);
    Task<IEnumerable<Player>> GetPlayerList(JsonElement condition, View options);
    Task<CUDMessage> AddPlayer(IPlayer newPlayer);
    Task<CUDMessage> UpdatePlayers(JsonElement condition, JsonElement token);
    Task<CUDMessage> DeletePlayer(string dbname);
  }
}