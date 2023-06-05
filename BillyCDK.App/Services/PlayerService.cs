using BillyCDK.App.Models;
using BillyCDK.App.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillyCDK.App.Services;

public class PlayerService : AbstractDataService<Player>, IPlayerService
{
    protected override IMongoCollection<Player> Collection { get; set; }
    protected override string EntityName { get; set; } = "player";

    public PlayerService(
        IDBCollection collection
    )
    {
        Collection = collection.Players;
    }

    public async Task<InstanceMessage<Player>> AddPlayers(IList<InputPlayer> newPlayers)
    {
        var insertingPlayers = (
            from player in newPlayers
            select new Player(
                ID: ObjectId.GenerateNewId().ToString(),
                DBName: string.IsNullOrWhiteSpace(player.DBName) ? $"{EntityName}-{Guid.NewGuid()}" : player.DBName,
                UpdatedDate: DateTime.Now,
                DeletedDate: null,
                Name: player.Name ?? "",
                Bio: player.Bio ?? "",
                IsPremium: player.IsPremium,
                Games: player.Games ?? new List<string>()
            )
        ).ToList();
        return await Add(insertingPlayers);
    }

    public async Task<InstanceMessage<Player>> AddGames(string playerDBName, List<string> games)
            => await AddItemsToList(
                "games",
                games,
                DBUtils.BuildSingleQueryCondition<Player>("dbname", playerDBName)
            );

}