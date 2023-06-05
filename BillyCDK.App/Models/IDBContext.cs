using MongoDB.Driver;

namespace BillyCDK.App.Models
{
  public interface IDBContext
  {
    IMongoDatabase DBInstance { get; }
    bool Drop();
  }
}
