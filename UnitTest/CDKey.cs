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
  public class CDKeyTest : IClassFixture<DbFixture>, IDisposable
  {
    private readonly ServiceProvider serviceProvider;
    private readonly ICDKeyService cdkeyService;
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;
    private readonly IDBContext dbContext;

    public CDKeyTest(DbFixture fixture)
    {
      serviceProvider = fixture.ServiceProvider;
      cdkeyService = serviceProvider.GetService<ICDKeyService>();
      gameService = serviceProvider.GetService<IGameService>();
      playerService = serviceProvider.GetService<IPlayerService>();
      dbContext = serviceProvider.GetService<IDBContext>();
    }

    public void Dispose()
    {
      // called after each test method
      dbContext.Drop();
    }

    // cdkeys has 4 keys (games).
    // cdk nums:
    // games[0].DBName -> 1,
    // games[1].DBName -> 2,
    // games[2].DBName -> 2,
    // games[3].DBName -> 1,
    [Theory(DisplayName = "CDkey Activation Test")]
    [ClassData(typeof(DataCDKeyActivate))]
    public async void TestActivate(
      Player player,
      List<Game> games,
      Dictionary<string, List<string>> cdkeys
    )
    {
      // prepare player + games
      CUDMessage addPlayerMessage = await playerService.Add(player);
      Assert.True(addPlayerMessage.OK);
      CUDMessage addGameMessage = await gameService.Add(games);
      Assert.True(addGameMessage.OK);

      // create CDK instances for the games according to the map (at this time: player is null)
      foreach(var pair in cdkeys) {

        foreach(var cdkValue in pair.Value) {
          var cdkey = new CDKey() {
            Game = pair.Key,
            Value = cdkValue,
            IsActivated = false,
          };
          CUDMessage addCDKMessage = await cdkeyService.Add(cdkey);
          Assert.True(addCDKMessage.OK);
        }

      }

      // Activate one cdk. Should be able to add, and player then has the game.
      // -> (player not null + appear in "games" field in player instance) We should get the cdk instance.
      InstanceCUDMessage<CDKey> activateMessage = await cdkeyService.Activate(player.DBName, cdkeys[games[0].DBName][0]);
      CDKey singleCDK = activateMessage.Instance;
      Assert.True(singleCDK.Player == player.DBName);
      Player playerInDB = await playerService.Get(player.DBName);
      Assert.NotNull(playerInDB.Games.Find(ele => ele == games[0].DBName));

      // Activate multiple cdks. Should be able to add, and player then has the list of games. We should get a list of cdk instance.
      var cdkValues = new List<string> {
        cdkeys[games[1].DBName][0],
        cdkeys[games[2].DBName][0],
      };
      activateMessage = await cdkeyService.Activate(player.DBName, cdkValues);
      List<CDKey> multipleCDKs = activateMessage.Instances;
      Assert.True(multipleCDKs[1].Player == player.DBName);
      playerInDB = await playerService.Get(player.DBName);
      Assert.True(playerInDB.Games.Count == 1 + 2);

      // Activate an invalid CDKey. Message should not be okay.
      activateMessage = await cdkeyService.Activate(player.DBName, "2GDH1-5BDK2-BILLY-3CDK4"); // 风暴英雄CDK
      Console.WriteLine(activateMessage.Message);
      Assert.False(activateMessage.OK);
      
      // Activate duplicate keys. Message should not be okay because:
      // 1. key already activated

      activateMessage = await cdkeyService.Activate(player.DBName, cdkeys[games[0].DBName][0]);
      Console.WriteLine(activateMessage.Message);
      Assert.False(activateMessage.OK);

      // 2. player already owns the game, activation should fail.
      activateMessage = await cdkeyService.Activate(player.DBName, cdkeys[games[1].DBName][1]);
      Console.WriteLine(activateMessage.Message);
      Assert.False(activateMessage.OK);
      CDKey cdkeyInDB = await cdkeyService.GetByValue(cdkeys[games[1].DBName][1]);
      //(await cdkeyService.Get(JsonDocument.Parse("{" + "\"value\": " + $"\"{cdkeys[games[1].DBName][1]}\"" + "}").RootElement))[0];
      Assert.False(cdkeyInDB.IsActivated);

      // Activate multiple keys contains duplications.
      // we should get a list of successfully added cdk instances,
      // and the games will only be added if player does not own those games
      cdkValues = new List<string> {
        cdkeys[games[2].DBName][0], // (key already activated)
        cdkeys[games[2].DBName][1], // duplicate game
        cdkeys[games[3].DBName][0], // good
      };

      activateMessage = await cdkeyService.Activate(player.DBName, cdkValues);
      multipleCDKs = activateMessage.Instances;
      Assert.True(multipleCDKs.Count == 1);

      // check game[2] should not be activated
      // "player" field of unsuccessfully added cdkeys should remain null
      cdkeyInDB = await cdkeyService.GetByValue(cdkeys[games[2].DBName][1]);
      // (await cdkeyService.Get(JsonDocument.Parse("{" + "\"value\": " + $"\"{cdkeys[games[2].DBName][1]}\"" + "}").RootElement))[0];
      Assert.False(cdkeyInDB.IsActivated);
      Assert.Null(cdkeyInDB.Player);

      // player should own game[3]
      playerInDB = await playerService.Get(player.DBName);
      Assert.NotNull(playerInDB.Games.Find(ele => ele == games[3].DBName));

    }

  public class DataCDKeyActivate : TheoryData<
    Player,
    List<Game>,
    Dictionary<string, List<string>>
  >
  {
    public DataCDKeyActivate()
    {
      Add(
        new Player()
        {
          DBName = "player-one",
          IsPremium = false,
          Games = new List<string>() { },
        },
        new List<Game> {
          new Game() { DBName = "game-halfLife_Alyx"},
          new Game() { DBName = "game-minecraftDungeons"},
          new Game() { DBName = "game-assassinsCreedValhalla"},
          new Game() { DBName = "game-happyDouDiZhu"},
        },
        new Dictionary<string, List<string>> () {
          // note: The following CDKeys are randomly generated with:
          //        randomcodegenerator.com/en/generate-codes@serial-numbers#result
          //       They are for testing purposes only within this program,
          //       and are INVALID on real-world game platforms
          ["game-halfLife_Alyx"] = new List<string>() {"WK7C-CMWD-HGEH-HSLE"},
          ["game-minecraftDungeons"] = new List<string>() {"F8YP-PWXM-HKPZ-EVR2", "6RBN-29JS-EDA2-8P4M"},
          ["game-assassinsCreedValhalla"] = new List<string>() {"2DXF-VGRJ-8U8G-5UF7", "R8X6-Y8G6-B5F4-DEBG"},
          ["game-happyDouDiZhu"] = new List<string>() {"UA9D-D3RE-NPGU-5TFH"},
        }
      );
    }
  }

  }
}