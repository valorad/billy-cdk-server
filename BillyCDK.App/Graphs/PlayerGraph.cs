using BillyCDK.App.Models;
using BillyCDK.App.Services;
using GraphQL;

namespace BillyCDK.App.Graphs;

public class PlayerGraph
{
}


public partial class Query
{

    [GraphQLMetadata("players")]
    public async Task<List<Player>> GetPlayers()
    {
        PlayerService svc = new();
        return await svc.Get();
    }

}

public partial class Mutation
{


}