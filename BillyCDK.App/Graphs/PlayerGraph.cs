﻿using BillyCDK.DataAccess.Models;
using BillyCDK.DataAccess.Services;
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
        //PlayerService svc = new();
        //return await svc.Get();
        throw new NotImplementedException();
    }

}

public partial class Mutation
{


}