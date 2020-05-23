using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface IPlayerService : IBaseDataService<Player>
  {
    Task<CUDMessage> AddGame(string player, string game);
    Task<CUDMessage> AddGames(string player, List<string> games);
  }
}