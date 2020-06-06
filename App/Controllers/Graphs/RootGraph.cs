using System;
using App.Models;
using App.Services;
using GraphQL.Types;

namespace App.Controllers.Graphs
{

  // public class RootGraph : Schema
  // {
  //   public RootGraph(IServiceProvider provider) : base(provider)
  //   {
  //     var schema = For(@"
  //       type Player {
  //         _id: ID!
  //         dbname: String!
  //         isPremium: Boolean!
  //         games: [String]!
  //       }

  //       input PlayerView {
  //         dbname: String!
  //         isPremium: Boolean!
  //       }

  //       type Query {
  //         player(dbname: String!): Player
  //       }
  //     ", _ =>
  //     {
  //       _.Types.Include<Query>();
  //       _.ServiceProvider = provider;
  //     });

  //     Query = schema.Query;
  //     Mutation = schema.Mutation;
  //     Subscription = schema.Subscription;
  //   }
  // }


  public partial class Query
  {
    private readonly IPlayerService playerService; 
    private readonly IGameService gameService;
    private readonly ICDKService cdkService;

    public Query(
      IPlayerService playerService,
      IGameService gameService,
      ICDKService cdkService
    )
    {
      this.playerService = playerService;
      this.gameService = gameService;
      this.cdkService = cdkService;
    }
  }

}