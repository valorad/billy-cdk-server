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
    Task<Player> GetPlayer(string dbname, IViewOption options);
    Task<List<Player>> GetPlayerList(JsonElement condition);
    Task<List<Player>> GetPlayerList(JsonElement condition, IViewOption options);
    Task<CUDMessage> AddPlayer(IPlayer newPlayer);
    Task<CUDMessage> AddPlayers(List<IPlayer> newPlayers);
    Task<CUDMessage> UpdatePlayer(string uniqueField, JsonElement token);
    Task<CUDMessage> UpdatePlayers(JsonElement condition, JsonElement token);
    Task<CUDMessage> DeletePlayer(string dbname);
    Task<CUDMessage> DeletePlayers(JsonElement condition);
  }
}