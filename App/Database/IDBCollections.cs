using App.Models;
using MongoDB.Driver;

namespace App.Database
{
  public interface IDBCollection
  {
    IMongoCollection<Player> Players { get; }
    IMongoCollection<Game> Games { get; }
    IMongoCollection<CDKey> CDKeys { get; }
  }
}