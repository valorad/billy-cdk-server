using System;
using System.Collections.Generic;
using System.Text.Json;
using App.Database;
using App.Models;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTest
{

  [Collection("Sequential")]
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
      // called after each test method
      dbContext.Drop();
    }

    [Theory(DisplayName = "Player singular test")]
    [ClassData(typeof(TestSingleData))]
    public async void TestCRUDSingle(Player newPlayer, JsonElement updateToken)
    {

      // Add single
      CUDMessage addMessage = await playerService.AddSingle(newPlayer);
      Assert.True(addMessage.OK);
      IPlayer playerInDB = await playerService.GetSingle(newPlayer.DBName);
      Assert.NotNull(playerInDB);
      // update single
      CUDMessage updateMessage = await playerService.UpdateSingle(newPlayer.DBName, updateToken);
      playerInDB = await playerService.GetSingle(newPlayer.DBName);
      Assert.Equal("game-tesV", playerInDB.Games[0]);
      // delete single
      CUDMessage deleteMessage = await playerService.DeleteSingle(newPlayer.DBName);
      Assert.True(deleteMessage.OK);
      playerInDB = await playerService.GetSingle(newPlayer.DBName);
      Assert.Null(playerInDB);

    }

    [Theory(DisplayName = "Player plural test")]
    [ClassData(typeof(TestListData))]
    public async void TestCRUDList(List<Player> newPlayers, JsonElement updateCondition, JsonElement updateToken)
    {

      // Add many
      CUDMessage addMessage = await playerService.AddList(newPlayers);
      Assert.True(addMessage.OK);
      List<Player> playersInDB = await playerService.GetList(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 3);
      // update many
      CUDMessage updateMessage = await playerService.UpdateList(updateCondition, updateToken);
      Assert.Equal(2, updateMessage.NumAffected);
      playersInDB = await playerService.GetList(JsonDocument.Parse("{}").RootElement);
      Assert.Equal(2, playersInDB[2].Games.Count);
      // delete many
      CUDMessage deleteMessage = await playerService.DeleteList(updateCondition);
      Assert.True(deleteMessage.NumAffected == 2);
      playersInDB = await playerService.GetList(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 1);
    }

  }

  public class TestSingleData : TheoryData<Player, JsonElement>
  {
    public TestSingleData()
    {
      Add(
        new Player()
        {
          DBName = "player-one",
          IsPremium = false,
          Games = new List<string>() { },
        },
        JsonDocument.Parse("{  \"$set\": {    \"games\": [\"game-tesV\"]  }}").RootElement
      );
    }
  }

  public class TestListData : TheoryData<List<Player>, JsonElement, JsonElement>
  {
    public TestListData()
    {

      Add(
        new List<Player>() {
          new Player() {
            DBName = "player-one",
            IsPremium = false,
            Games = new List<string>() { },
          },
          new Player() {
            DBName = "player-squrriel",
            IsPremium = true,
            Games = new List<string>() { },
          },
          new Player() {
            DBName = "player-pcmasterrace",
            IsPremium = true,
            Games = new List<string>() { },
          }
        },

        JsonDocument.Parse("{\"isPremium\": true}").RootElement,
        JsonDocument.Parse("{  \"$set\": {    \"games\": [      \"game-terraria\",      \"game-kittyparty\"    ]  }}").RootElement
      );
    }
  }

}