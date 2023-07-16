using MongoDB.Driver;

namespace BillyCDK.App.Models;

public interface IDBCollection
{
    IMongoCollection<Player> Players { get; }
    IMongoCollection<Game> Games { get; }
    IMongoCollection<CDKey> CDKeys { get; }
}
