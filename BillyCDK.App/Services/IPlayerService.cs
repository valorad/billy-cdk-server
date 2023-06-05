using BillyCDK.App.Models;

namespace BillyCDK.App.Services;
public interface IPlayerService : IAbstractDataService<Player>
{
    Task<InstanceMessage<Player>> AddGames(string playerDBName, List<string> games);
    Task<InstanceMessage<Player>> AddPlayers(IList<InputPlayer> newPlayers);
}