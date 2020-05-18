using App.Models;
using MongoDB.Driver;

namespace App.Database
{
  public interface IDBContext
  {
    IMongoCollection<Player> Players { get; }

    bool Drop();
    // IMongoCollection<Game> Games { get; }
    // IMongoCollection<CDKey> CDKeys { get; }
  }
}