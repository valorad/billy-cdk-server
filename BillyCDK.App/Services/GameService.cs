using BillyCDK.App.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillyCDK.App.Services;

public class GameService : AbstractDataService<Game>, IGameService
{
    protected override IMongoCollection<Game> Collection { get; set; }
    protected override string EntityName { get; set; } = "game";

    public GameService(
        IDBCollection collection
    )
    {
        Collection = collection.Games;
    }

    public async Task<InstanceMessage<Game>> AddGames(IList<InputGame> newGames)
    {
        var insertingGames = (
            from game in newGames
            select new Game(
                ID: ObjectId.GenerateNewId().ToString(),
                DBName: string.IsNullOrWhiteSpace(game.DBName) ? $"{EntityName}-{Guid.NewGuid()}" : game.DBName,
                UpdatedDate: DateTime.Now,
                DeletedDate: null,
                Name: game.Name ?? "",
                Description: game.Description ?? "",
                Price: game.Price ?? default
            )
        ).ToList();
        return await Add(insertingGames);
    }

}