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
      CUDMessage addMessage = await playerService.Add(newPlayer);
      Assert.True(addMessage.OK);
      IPlayer playerInDB = await playerService.Get(newPlayer.DBName);
      Assert.NotNull(playerInDB);
      // update single
      CUDMessage updateMessage = await playerService.Update(newPlayer.DBName, updateToken);
      playerInDB = await playerService.Get(newPlayer.DBName);
      Assert.Equal("game-tesV", playerInDB.Games[0]);
      // delete single
      CUDMessage deleteMessage = await playerService.Delete(newPlayer.DBName);
      Assert.True(deleteMessage.OK);
      playerInDB = await playerService.Get(newPlayer.DBName);
      Assert.Null(playerInDB);

    }

    [Theory(DisplayName = "Player plural test")]
    [ClassData(typeof(TestListData))]
    public async void TestCRUDList(List<Player> newPlayers, JsonElement updateCondition, JsonElement updateToken)
    {

      // Add many
      CUDMessage addMessage = await playerService.Add(newPlayers);
      Assert.True(addMessage.OK);
      List<Player> playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 3);
      // update many
      CUDMessage updateMessage = await playerService.Update(updateCondition, updateToken);
      Assert.Equal(2, updateMessage.NumAffected);
      playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement);
      Assert.Equal(2, playersInDB[2].Games.Count);
      // delete many
      CUDMessage deleteMessage = await playerService.Delete(updateCondition);
      Assert.True(deleteMessage.NumAffected == 2);
      playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement);
      Assert.True(playersInDB.Count == 1);
    }

    [Theory(DisplayName = "Player add game test")]
    [ClassData(typeof(TestAddGameData))]
    public async void TestAddGame(Player newPlayer, string aGame, List<string> moreGames)
    {

      // Add a player
      CUDMessage addMessage = await playerService.Add(newPlayer);
      Assert.True(addMessage.OK);

      // Add a game
      CUDMessage addGameMessage = await playerService.AddGame(newPlayer.DBName, aGame);
      Assert.True(addGameMessage.OK);

      // Add more games
      addGameMessage = await playerService.AddGames(newPlayer.DBName, moreGames);
      Assert.True(addGameMessage.OK);

      // Game item check
      Player playerInDB = await playerService.Get(newPlayer.DBName);
      // Last game item should be the same as the last added game item in "moreGames"
      Assert.True(playerInDB.Games[moreGames.Count] == moreGames[moreGames.Count - 1]);

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

  public class TestAddGameData : TheoryData<Player, string, List<string>>
  {
    public TestAddGameData()
    {
      Add(
        new Player()
        {
          DBName = "player-one",
          IsPremium = false,
          Games = new List<string>() { },
        },
        "game-halo6",
        new List<string> () {"game-cod16", "game-elderScrollBlades", "game-animalCrossing"}
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