using BillyCDK.DataAccess.Models;

namespace BillyCDK.DataAccess.Services;

public interface IGameService : IAbstractDataService<Game>
{
    Task<InstanceMessage<Game>> AddGames(IList<InputGame> newGames);
}
