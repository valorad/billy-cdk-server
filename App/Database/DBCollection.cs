using App.Models;
using MongoDB.Driver;

namespace App.Database
{
  public class DBCollection : IDBCollection
  {
    private readonly IDBContext context;
    private readonly IMongoDatabase dbInstance;

    public DBCollection(IDBContext context)
    {
      this.context = context;
      this.dbInstance = context.GetDatabase();
    }

    public IMongoCollection<Player> Players => dbInstance.GetCollection<Player>("players");
    public IMongoCollection<Game> Games => dbInstance.GetCollection<Game>("games");
    public IMongoCollection<CDKey> CDKeys => dbInstance.GetCollection<CDKey>("cdkeys");
  }
}