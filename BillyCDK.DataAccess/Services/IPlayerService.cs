using BillyCDK.DataAccess.Models;

namespace BillyCDK.DataAccess.Services;

public interface IPlayerService : IAbstractDataService<Player>
{
    Task<InstanceMessage<Player>> AddGames(string playerDBName, List<string> games);
    Task<InstanceMessage<Player>> AddPlayers(IList<InputPlayer> newPlayers);
}