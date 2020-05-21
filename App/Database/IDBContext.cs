using App.Models;
using MongoDB.Driver;

namespace App.Database
{
  public interface IDBContext
  {

    bool Drop();
    IMongoDatabase GetDatabase();

  }
}