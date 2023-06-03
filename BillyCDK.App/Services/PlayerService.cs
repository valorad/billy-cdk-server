using BillyCDK.App.Models;
using BillyCDK.App.Utilities;
using MongoDB.Driver;

namespace BillyCDK.App.Services;

public class PlayerService : AbstractDataService<Player>
{
    protected override IMongoCollection<Player> Collection { get; set; }
    protected override string EntityName { get; set; } = "player";

    public PlayerService(
        IDBCollection collection
    )
    {
        Collection = collection.Players;
    }

    public async Task<InstanceMessage<Player>> AddGame(string playerDBName, List<string> games)
            => await AddItemsToList(
                "games",
                games,
                DBUtils.BuildSingleQueryCondition<Player>("dbname", playerDBName)
            );

}