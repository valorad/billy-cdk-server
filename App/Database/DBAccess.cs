using System;
using System.Text.RegularExpressions;
using App.Models;
using MongoDB.Driver;

namespace App.Database
{
  public class DBAccess : IDBContext
  {
    private readonly IDBConfig settings;
    private readonly MongoClient client;
    private readonly IMongoDatabase dbInstance;

    // private readonly string uri;

    public DBAccess(IDBConfig settings)
    {
      this.settings = settings;

      string uri = @$"
        mongodb://{ settings.User }
        :{ settings.Password }
        @{ settings.Host }
        /{ settings.DataDB }
        ?authSource={ settings.AuthDB }
      ";

      // remove all white spaces
      uri = Regex.Replace(uri, @"\s+", "");

      Console.WriteLine($"uri: {uri}");

      client = new MongoClient(uri);

      dbInstance = client.GetDatabase(settings.DataDB);
    }

    // private IMongoDatabase Connect()
    // {
    //   client = new MongoClient(uri);
    //   return client.GetDatabase(settings.DB.Data);
    // }

    private bool Drop() {
      try
      {
        this.client.DropDatabase(settings.DataDB);
        return true;
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        return false;
      }
      
    }

    public IMongoCollection<Player> Players => dbInstance.GetCollection<Player>("players");
  }
}