using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface IPlayerService : IBaseDataService<Player>
  {
    # region AddGame
    Task<CUDMessage> AddGame(string player, string game);
    Task<CUDMessage> AddGame(string player, List<string> games);
    Task<CUDMessage> AddGame(Player player, string game);
    Task<CUDMessage> AddGame(Player player, List<string> games);
    Task<CUDMessage> AddGame(Player player, Game game);
    Task<CUDMessage> AddGame(Player player, List<Game> games);
    #endregion
  }
}