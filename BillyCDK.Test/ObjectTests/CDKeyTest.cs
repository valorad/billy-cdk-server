using BillyCDK.DataAccess.Models;
using BillyCDK.DataAccess.Services;
using BillyCDK.DataAccess.Utilities;
using BillyCDK.Test.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillyCDK.Test.ObjectTests;

[Collection("Sequential")]
public class CDKeyTest : IClassFixture<ServiceFixture>, IDisposable
{
    private readonly IHost testHost;
    private readonly IDBContext dbContext;
    private readonly ICDKeyService cdkeyService;
    private readonly IPlayerService playerService;
    private readonly IGameService gameService;

    public CDKeyTest(ServiceFixture fixture)
    {
        testHost = fixture.TestHost;
        dbContext = testHost.Services.GetRequiredService<IDBContext>();
        cdkeyService = testHost.Services.GetRequiredService<ICDKeyService>();
        gameService = testHost.Services.GetRequiredService<IGameService>();
        playerService = testHost.Services.GetRequiredService<IPlayerService>();
    }

    public void Dispose()
    {
        // called after each test method
        dbContext.Drop();
        GC.SuppressFinalize(this);
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
        InputPlayer player,
        List<InputGame> games,
        Dictionary<string, List<string>> cdkeys
    )
    {
        // prepare player + games
        InstanceMessage<Player> addPlayerMessage = await playerService.AddPlayers(new List<InputPlayer>() { player });
        Assert.Equal(1, addPlayerMessage.Okay);
        InstanceMessage<Game> addGameMessage = await gameService.AddGames(games);
        Assert.Equal(1, addGameMessage.Okay);

        // create CDK instances for the games according to the map (at this time: player is null)
        List<InputCDKey> newCDKeys = new();
        foreach (var (key, value) in cdkeys) {
            foreach(var cdkValue in value) {
                var cdkey = new InputCDKey(
                    Value: cdkValue,
                    Player: null,
                    Game: key,
                    IsActivated: false,
                    Price: null,
                    Platform: null
                );
                newCDKeys.Add(cdkey);
            }
        }

        InstanceMessage<CDKey> addCDKMessage = await cdkeyService.AddCDKeys(newCDKeys);
        Assert.Equal(1, addCDKMessage.Okay);

        // Activate one cdk. Should be able to add, and player then has the game.
        // -> (player not null + appear in "games" field in player instance) We should get the cdk instance.
        InstanceMessage<CDKey> activateMessage = await cdkeyService.Activate(
            player.DBName,
            new List<string>() { cdkeys[games[0].DBName!][0] }
        );
        Assert.NotNull(activateMessage.Instances);
        // The instances contains only _id and DBName, not including Player dbname

        CDKey singleCDK = (await cdkeyService.Get(
            DBUtils.BuildSingleQueryCondition<CDKey>("_id", activateMessage.Instances.First().ID)
        )).First();

        Assert.Equal(player.DBName, singleCDK.Player);

        Player playerInDB = (await playerService.Get(
            DBUtils.BuildSingleQueryCondition<Player>("dbname", player.DBName)
        )).First();
        Assert.NotNull(playerInDB.Games.Find(ele => ele == games[0].DBName));

        // Activate multiple cdks. Should be able to add, and player then has the list of games. We should get a list of cdk instance.
        var cdkValues = new List<string> {
            cdkeys[games[1].DBName].First(),
            cdkeys[games[2].DBName].First(),
        };
        activateMessage = await cdkeyService.Activate(player.DBName, cdkValues);
        Assert.NotNull(activateMessage.Instances);

        var cdkeyIDs = (
            from cdkey in activateMessage.Instances
            select ObjectId.Parse(cdkey.ID)
        ) ;

        IList<CDKey> multipleCDKs = await cdkeyService.Get(
            Builders<CDKey>.Filter.In("_id", cdkeyIDs)
        );

        Assert.True(multipleCDKs[1].Player == player.DBName);
        playerInDB = (await playerService.Get(
            DBUtils.BuildSingleQueryCondition<Player>("dbname", player.DBName)
        )).First();
        Assert.Equal(1 + 2, playerInDB.Games.Count);

        // Activate an invalid CDKey. Message should not be okay.
        activateMessage = await cdkeyService.Activate(
            player.DBName,
            new List<string>() { "2GDH1-5BDK2-BILLY-3CDK4" } // 风暴英雄CDK
        );
        Assert.Contains("CDKey is invalid", activateMessage.Message);
        Assert.Equal(0, activateMessage.Okay);
      
        // Activate duplicate keys. Message should not be okay because:
        // 1. key already activated
        activateMessage = await cdkeyService.Activate(
            player.DBName,
            new List<string>() { cdkeys[games[0].DBName].First() }
        );
        Assert.Contains("CDKey is invalid", activateMessage.Message);
        Assert.Equal(0, activateMessage.Okay);

        // 2. player already owns the game, activation should fail.
        activateMessage = await cdkeyService.Activate(
            player.DBName,
            new List<string>() { cdkeys[games[0].DBName].First() }
        );
        Assert.Contains("CDKey is invalid", activateMessage.Message);
        Assert.Equal(0, activateMessage.Okay);

        CDKey cdkeyInDB = (await cdkeyService.GetByValues(
            new List<string>() { cdkeys[games[1].DBName][1] }
        )).First();
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
        Assert.NotNull(activateMessage.Instances);
        multipleCDKs = activateMessage.Instances!;
        Assert.Equal(1, multipleCDKs.Count);

        // check game[2] should not be activated
        // "player" field of unsuccessfully added cdkeys should remain null
        cdkeyInDB = (await cdkeyService.GetByValues(
            new List<string>() { cdkeys[games[2].DBName][1] }
        )).First();
        Assert.False(cdkeyInDB.IsActivated);
        Assert.Null(cdkeyInDB.Player);

        // player should own game[3]
        playerInDB = (await playerService.Get(
            DBUtils.BuildSingleQueryCondition<Player>("dbname", player.DBName)
        )).First();
        Assert.NotNull(playerInDB.Games.Find(ele => ele == games[3].DBName));

    }

    public class DataCDKeyActivate : TheoryData<
        InputPlayer,
        List<InputGame>,
        Dictionary<string, List<string>>
    >
    {
        public DataCDKeyActivate()
        {
            Add(
                new InputPlayer(
                    DBName: "player-one",
                    Name: null,
                    Bio: null,
                    IsPremium: false,
                    Games: new ()
                ),
                new List<InputGame>()
                {
                    new InputGame(
                        DBName: "game-halfLife_Alyx",
                        Name: null,
                        Description: null,
                        Price: null
                    ),
                    new InputGame(
                        DBName: "game-minecraftDungeons",
                        Name: null,
                        Description: null,
                        Price: null
                    ),
                    new InputGame(
                        DBName: "game-assassinsCreedValhalla",
                        Name: null,
                        Description: null,
                        Price: null
                    ),
                    new InputGame(
                        DBName: "game-happyDouDiZhu",
                        Name: null,
                        Description: null,
                        Price: null
                    ),
                },
                new Dictionary<string, List<string>>()
                {
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
