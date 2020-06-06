using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace App.Controllers.Graphs
{

  // public class PlayerGraph : Schema
  // {

  //   private IPlayerService playerService { get; }
  //   // resolvers
  //   public PlayerGraph(IPlayerService playerService)
  //   {
  //     this.playerService = playerService;
  //   }

  //   public Task<Player> Get(string dbname)
  //   {
  //     return playerService.Get(dbname);
  //   }

  // }

  public partial class Query
  {

    [GraphQLMetadata("player")]
    public async Task<Player> GetPlayer(string dbname)
    {
      Console.WriteLine(playerService);
      Player player = await playerService.Get(dbname);
      return player; 
      // return new Player() {ID="aaa", DBName = "yes", Games = new List<string>() {}};
    }
  }

}