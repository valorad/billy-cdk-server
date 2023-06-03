using MongoDB.Driver;

namespace BillyCDK.App.Database
{
  public interface IDBContext
  {
    IMongoDatabase DBInstance { get; }
    bool Drop();
  }
}
