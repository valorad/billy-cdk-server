using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using GraphQL;

namespace App.Controllers.Graphs
{
  public class GameGraph : RootGraph<Game>
  {
    public GameGraph(IGameService gameService) : base(gameService)
    {
      this.itemName = "game";

      this.defaultField = "dbname";

      this.uniqueFields = new List<string>() {
        "dbname"
      };

      this.requiredFields = new List<string>() {
        "dbname"
      };
    }

  }


  public partial class Query
  {

    [GraphQLMetadata("game")]
    public async Task<Game> GetGame(string dbname, string options)
    {
      return await gameGraph.GetSingle(dbname, options);
    }

    [GraphQLMetadata("games")]
    public async Task<List<Game>> GetGames(string condition, string options)
    {
      return await gameGraph.GetList(condition, options);
    }

  }

  public partial class Mutation
  {

    [GraphQLMetadata("addGame")]
    public async Task<CUDMessage> AddGame(Game newGame)
    {
      return await gameGraph.AddSingle(newGame);
    }

    [GraphQLMetadata("addGames")]
    public async Task<CUDMessage> AddGame(List<Game> newGames)
    {
      return await gameGraph.AddList(newGames);
    }

    [GraphQLMetadata("updateGame")]
    public async Task<CUDMessage> UpdateGame(string dbname, string token)
    {
      return await gameGraph.UpdateSingle(dbname, token);
    }

    [GraphQLMetadata("updateGames")]
    public async Task<CUDMessage> UpdateGames(string condition, string token)
    {
      return await gameGraph.UpdateList(condition, token);
    }

    [GraphQLMetadata("deleteGame")]
    public async Task<CUDMessage> DeleteGame(string dbname)
    {
      return await gameGraph.DeleteSingle(dbname);
    }

    [GraphQLMetadata("deleteGames")]
    public async Task<CUDMessage> DeleteGames(string condition)
    {
      return await gameGraph.DeleteList(condition);
    }

  }



}