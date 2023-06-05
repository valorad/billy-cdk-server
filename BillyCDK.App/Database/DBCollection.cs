using BillyCDK.App.Models;
using MongoDB.Driver;

namespace BillyCDK.App.Database
{
  public class DBCollection : IDBCollection
  {
    private readonly IDBContext context;
    private readonly IMongoDatabase dbInstance;

    public DBCollection(IDBContext context)
    {
      this.context = context;
      dbInstance = context.DBInstance;
    }

    public IMongoCollection<Player> Players => dbInstance.GetCollection<Player>("players");
  }
}