using MongoDB.Driver;

namespace BillyCDK.DataAccess.Models
{
  public interface IDBContext
  {
    IMongoDatabase DBInstance { get; }
    bool Drop();
  }
}
