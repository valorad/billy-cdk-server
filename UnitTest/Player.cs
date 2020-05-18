using System;
using System.Collections.Generic;
using System.Reflection;
using App.Database;
using App.Models;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace UnitTest
{


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

    private readonly ServiceProvider serviceProvider;
    private readonly IPlayerService playerService;
    private readonly IDBContext dbContext;

    public PlayerTest(DbFixture fixture)
    {
      serviceProvider = fixture.ServiceProvider;
      playerService = serviceProvider.GetService<IPlayerService>();
      dbContext = serviceProvider.GetService<IDBContext>();
    }

    public void Dispose()
    {
      dbContext.Drop();
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