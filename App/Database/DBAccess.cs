using System;
using System.Text.RegularExpressions;
using MongoDB.Driver;

namespace App.Database
{
  public class DBAccess : IDBContext
  {
    private readonly IDBConfig settings;
    private readonly IMongoDatabase dbInstance;

    private readonly string uri;

    public DBAccess(IDBConfig settings)
    {
      this.settings = settings;

      uri = @$"
        mongodb://{ settings.User }
        :{ settings.Password }
        @{ settings.Host }
        /{ settings.DB.Data }
        ?authSource={ settings.DB.Auth }
      ";

      // remove all white spaces
      uri = Regex.Replace(uri, @"\s+", "");

      Console.WriteLine($"uri: {uri}");

      dbInstance = Connect();
    }

    private IMongoDatabase Connect()
    {
      var client = new MongoClient(uri);
      return client.GetDatabase(settings.DB.Data);
    }

    public IMongoCollection<Player> Players => dbInstance.GetCollection<Player>("players");
  }
}