using App.Database;
using App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UnitTest
{
  public class DbFixture
  {
    public DbFixture()
    {
      var config = new ConfigurationBuilder()
          .AddYamlFile("secrets.yaml")
          .AddEnvironmentVariables()
          .Build();

      var services = new ServiceCollection();

      // add secrets
      services.Configure<DBConfig>(
        config.GetSection("mongo")
      );

      services.AddSingleton<IDBConfig>(sp =>
        sp.GetRequiredService<IOptions<DBConfig>>().Value
      );

      // config db
      services.AddTransient<IDBContext, DBAccess>();
      services.AddTransient<IDBCollection, DBCollection>();

      // add endpoints
      services.AddSingleton<IPlayerService, PlayerService>();
      services.AddSingleton<IGameService, GameService>();
      services.AddSingleton<ICDKeyService, CDKeyService>();

      ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; private set; }

  }
}