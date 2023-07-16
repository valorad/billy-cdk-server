using BillyCDK.App.Models;

namespace BillyCDK.App.Services;

public interface IGameService : IAbstractDataService<Game>
{
    Task<InstanceMessage<Game>> AddGames(IList<InputGame> newGames);
}
