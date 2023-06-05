using System.Text.Json;
using BillyCDK.App.Models;
using BillyCDK.App.Services;
using BillyCDK.App.Utilities;
using BillyCDK.Test.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BillyCDK.Test.ObjectTests;

[Collection("Sequential")]
public class PlayerTest : IClassFixture<ServiceFixture>, IDisposable
{
    private readonly IHost testHost;
    private readonly IPlayerService playerService;
    private readonly IDBContext dbContext;

    public PlayerTest(ServiceFixture fixture)
    {
        testHost = fixture.TestHost;
        dbContext = testHost.Services.GetRequiredService<IDBContext>();
        playerService = testHost.Services.GetRequiredService<IPlayerService>();
    }

    public void Dispose()
    {
        // called after each test method
        dbContext.Drop();
        GC.SuppressFinalize(this);
    }


    [Theory(DisplayName = "Player plural test")]
    [ClassData(typeof(DataMultiplePlayers))]
    public async void TestCRUDList(List<InputPlayer> newPlayers, JsonElement updateCondition, JsonElement updateToken)
    {
        // Add many
        InstanceMessage<Player> addMessage = await playerService.AddPlayers(newPlayers);
        Assert.Equal(1, addMessage.Okay);
        List<Player> playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement.ToString());
        Assert.True(playersInDB.Count == 3);
        // update many
        InstanceMessage<Player> updateMessage = await playerService.Update(updateCondition.ToString(), updateToken.ToString());
        Assert.Equal(2, updateMessage.NumAffected);
        playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement.ToString());
        Assert.Equal(2, playersInDB[2].Games.Count);
        // drop many
        InstanceMessage<Player> dropMessage = await playerService.Drop(updateCondition.ToString());
        Assert.True(dropMessage.NumAffected == 2);
        playersInDB = await playerService.Get(JsonDocument.Parse("{}").RootElement.ToString());
        Assert.True(playersInDB.Count == 1);
    }

    [Theory(DisplayName = "Player add game test")]
    [ClassData(typeof(DataPlayerAddGame))]
    public async void TestAddGame(InputPlayer newPlayer, string aGame, List<string> moreGames)
    {
        // Add a player
        InstanceMessage<Player> addMessage = await playerService.AddPlayers(new List<InputPlayer>() { newPlayer });
        Assert.Equal(1, addMessage.Okay);

        // Add a game
        InstanceMessage<Player> addGameMessage = await playerService.AddGames(newPlayer.DBName, new List<string>() { aGame } );
        Assert.Equal(1, addGameMessage.Okay);

        // Add more games
        addGameMessage = await playerService.AddGames(newPlayer.DBName, moreGames);
        Assert.Equal(1, addGameMessage.Okay);

        // Game item check
        Player playerInDB = (await playerService.Get(
            DBUtils.BuildSingleQueryCondition<Player>("dbname", newPlayer.DBName)
        )).First();
        // Last game item should be the same as the last added game item in "moreGames"
        Assert.True(playerInDB.Games[^1] == moreGames[^1]);
    }

}

public class DataPlayerAddGame : TheoryData<InputPlayer, string, List<string>>
{
    public DataPlayerAddGame()
    {
        Add(
            new InputPlayer(
                DBName: "player-one",
                Name: null,
                Bio: null,
                IsPremium: false,
                Games: null
            ),
            "game-halo6",
            new List<string>() { "game-cod16", "game-elderScrollBlades", "game-animalCrossing" }
        );
    }
}

public class DataMultiplePlayers : TheoryData<List<InputPlayer>, JsonElement, JsonElement>
{
    public DataMultiplePlayers()
    {

        Add(
            new List<InputPlayer>() {
                new InputPlayer(
                    DBName: "player-one",
                    Name: null,
                    Bio: null,
                    IsPremium: false,
                    Games: null
                ),
                new InputPlayer(
                    DBName: "player-squrriel",
                    Name: null,
                    Bio: null,
                    IsPremium: true,
                    Games: null
                ),
                new InputPlayer(
                    DBName: "player-pcmasterrace",
                    Name: null,
                    Bio: null,
                    IsPremium: true,
                    Games: null
                ),
            },

            JsonDocument.Parse("{\"isPremium\": true}").RootElement,
            JsonDocument.Parse("{  \"$set\": {    \"games\": [      \"game-terraria\",      \"game-kittyparty\"    ]  }}").RootElement
        );
    }
}