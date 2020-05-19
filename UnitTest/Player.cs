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
    public async void TestCRUDSingle(IPlayer newPlayer, JsonElement updateToken)
    {

      // Add single
      CUDMessage addMessage = await playerService.AddPlayer(newPlayer);
      Assert.True(addMessage.OK);
      IPlayer playerInDB = await playerService.GetPlayer(newPlayer.DBName);
      Assert.NotNull(playerInDB);
      // update single
      CUDMessage updateMessage = await playerService.UpdatePlayer(newPlayer.DBName, updateToken);
      playerInDB = await playerService.GetPlayer(newPlayer.DBName);
      Assert.Equal("game-tesV", playerInDB.Games[0]);
      // delete single
      CUDMessage deleteMessage = await playerService.DeletePlayer(newPlayer.DBName);
      Assert.True(deleteMessage.OK);
      playerInDB = await playerService.GetPlayer(newPlayer.DBName);
      Assert.Null(playerInDB);
    }

    [Theory(DisplayName = "Player plural test")]
    [ClassData(typeof(TestListData))]
    public async void TestCRUDList(List<IPlayer> newPlayers, JsonElement updateCondition, JsonElement updateToken)
    {

      // Add many
      CUDMessage addMessage = await playerService.AddPlayers(newPlayers);
      Assert.True(addMessage.OK);
      List<Player> playersInDB = await playerService.GetPlayerList(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 3);
      // update many
      CUDMessage updateMessage = await playerService.UpdatePlayers(updateCondition, updateToken);
      Assert.Equal(2, updateMessage.NumAffected);
      playersInDB = await playerService.GetPlayerList(JsonDocument.Parse("{}").RootElement);
      Assert.Equal(2, playersInDB[2].CDKeys.Count);
      // delete many
      CUDMessage deleteMessage = await playerService.DeletePlayers(updateCondition);
      Assert.True(deleteMessage.NumAffected == 2);
      playersInDB = await playerService.GetPlayerList(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 1);
    }

  }

  public class TestSingleData : TheoryData<IPlayer, JsonElement>
  {
    public TestSingleData()
    {
      Add(
        new Player()
        {
          DBName = "player-one",
          IsPremium = false,
          CDKeys = new List<string>() { },
          Games = new List<string>() { },
        },
        JsonDocument.Parse("{  \"$set\": {    \"games\": [\"game-tesV\"]  }}").RootElement
      );
    }
  }

  public class TestListData : TheoryData<List<IPlayer>, JsonElement, JsonElement>
  {
    public TestListData()
    {

      Add(
        new List<IPlayer>() {
          new Player() {
            DBName = "player-one",
            IsPremium = false,
            CDKeys = new List<string>() { },
            Games = new List<string>() { },
          },
          new Player() {
            DBName = "player-squrriel",
            IsPremium = true,
            CDKeys = new List<string>() { },
            Games = new List<string>() { },
          },
          new Player() {
            DBName = "player-pcmasterrace",
            IsPremium = true,
            CDKeys = new List<string>() { },
            Games = new List<string>() { },
          }
        },
        JsonDocument.Parse("{\"isPremium\": true}").RootElement,
        JsonDocument.Parse("{  \"$set\": {    \"cdKeys\": [      \"6aff50mongoObjectIDcreative\",      \"6aff51mongoObjectIDimo\"    ]  }}").RootElement
      );
    }
  }

}