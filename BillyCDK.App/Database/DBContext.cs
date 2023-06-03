using MongoDB.Driver;

namespace BillyCDK.App.Database;

public class DBContext : IDBContext
{
    private readonly IConfiguration config;
    private MongoUrl ClientURL { get; init; }
    private MongoClient Client { get; init; }
    public IMongoDatabase DBInstance { get; private init; }
    public DBContext(IConfiguration config)
    {
        this.config = config;
        ClientURL = new MongoUrl(config.GetConnectionString("DefaultConnection"));
        Client = new MongoClient(ClientURL);
        DBInstance = Client.GetDatabase(ClientURL.DatabaseName);
    }
    public bool Drop()
    {
        try
        {
            Client.DropDatabase(ClientURL.DatabaseName);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return false;
        }
    }

}