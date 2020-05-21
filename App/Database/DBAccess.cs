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

      client = new MongoClient(uri);

      dbInstance = client.GetDatabase(settings.DataDB);
    }

    public bool Drop() {
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

    public IMongoDatabase GetDatabase() {
      return dbInstance;
    }

  }
}