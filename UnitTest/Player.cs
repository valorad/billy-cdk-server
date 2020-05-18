using System;
using System.Collections.Generic;
using System.Reflection;
using App.Database;
using App.Models;
using App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Sdk;

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
      // serviceCollection
      //     .AddDbContext<SomeContext>(options => options.UseSqlServer("connection string"),
      //         ServiceLifetime.Transient);

      var d = config.GetSection("mongo");
      var c = d.GetSection("host");

      // add secrets
      services.Configure<DBConfig>(
        config.GetSection("mongo")
      );

      services.AddSingleton<IDBConfig>(sp =>
        sp.GetRequiredService<IOptions<DBConfig>>().Value
      );

      // config db
      services.AddTransient<IDBContext, DBAccess>();

      // add endpoints
      services.AddSingleton<IPlayerService, PlayerService>();

      ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; private set; }

    // public void Dispose()
    // {
    //   throw new NotImplementedException();
    // }
  }


  // [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  // public class TestBeforeAfter : BeforeAfterTestAttribute
  // {
  //   public override void Before(MethodInfo methodUnderTest)
  //   {
  //     // Debug.WriteLine(methodUnderTest.Name);

  //   }

  //   public override void After(MethodInfo methodUnderTest)
  //   {

  //     // Debug.WriteLine(methodUnderTest.Name);
  //   }
  // }


  // [TestBeforeAfter]
  public class PlayerTest : IClassFixture<DbFixture>, IDisposable
  {

    private readonly ServiceProvider _serviceProvider;
    private readonly IPlayerService playerService;
    private readonly IDBContext context;

    public PlayerTest(
      DbFixture fixture
    )
    {
      _serviceProvider = fixture.ServiceProvider;
      playerService = _serviceProvider.GetService<IPlayerService>();
      context = _serviceProvider.GetService<IDBContext>();
    }

    public void Dispose()
    {
      context.Drop();
    }

    [Theory(DisplayName = "Add + Get a player")]
    [ClassData(typeof(AddPlayerData))]
    public async void TestAdd(IPlayer newPlayer)
    {
      CUDMessage addMessage = await playerService.AddPlayer(newPlayer);
      Assert.True(addMessage.OK);
      IPlayer playerInDB = await playerService.GetPlayer(newPlayer.DBName);
      Assert.NotNull(playerInDB);
    }
  }

  public class AddPlayerData : TheoryData<IPlayer>
  {
    public AddPlayerData()
    {
      Add(
        new Player()
        {
          DBName = "player-one",
          IsPremium = false,
          CDKeys = new List<string>() { },
          Games = new List<string>() { },
        }
      );
    }
  }
}