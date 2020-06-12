using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Lib;
using App.Models;
using App.Services;
using GraphQL;

namespace App.Controllers.Graphs
{
  public class PlayerGraph : RootGraph<Player>
  {

    public PlayerGraph(IPlayerService playerService): base(playerService)
    {

      this.itemName = "player";

      this.defaultField = "dbname";

      this.uniqueFields = new List<string>() {
        "dbname"
      };

      this.requiredFields = new List<string>() {
        "dbname"
      };
    }

    // specify the default values to fill
    public override void Polyfill(Player newPlayer)
    {

      if (newPlayer.Games is null)
      {
        newPlayer.Games = new List<string>() { };
      }

    }

  }

  public partial class Query
  {

    [GraphQLMetadata("player")]
    public async Task<Player> GetPlayer(string dbname, string options)
    {
      return await playerGraph.GetSingle(dbname, options);
    }

    [GraphQLMetadata("players")]
    public async Task<List<Player>> GetPlayers(string condition, string options)
    {
      return await playerGraph.GetList(condition, options);

    }

  }

  public partial class Mutation
  {

    [GraphQLMetadata("addPlayer")]
    public async Task<CUDMessage> AddPlayer(Player newPlayer)
    {
      return await playerGraph.AddSingle(newPlayer);
    }

    [GraphQLMetadata("addPlayers")]
    public async Task<CUDMessage> AddPlayer(List<Player> newPlayers)
    {
      return await playerGraph.AddList(newPlayers);
    }

    [GraphQLMetadata("updatePlayer")]
    public async Task<CUDMessage> UpdatePlayer(string dbname, string token)
    {
      return await playerGraph.UpdateSingle(dbname, token);
    }

    [GraphQLMetadata("updatePlayers")]
    public async Task<CUDMessage> UpdatePlayers(string condition, string token)
    {
      return await playerGraph.UpdateList(condition, token);
    }

    [GraphQLMetadata("deletePlayer")]
    public async Task<CUDMessage> DeletePlayer(string dbname)
    {
      return await playerGraph.DeleteSingle(dbname);
    }

    [GraphQLMetadata("deletePlayers")]
    public async Task<CUDMessage> DeletePlayers(string condition)
    {
      return await playerGraph.DeleteList(condition);
    }

  }

}