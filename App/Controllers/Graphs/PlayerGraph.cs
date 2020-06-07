using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;
using GraphQL;

namespace App.Controllers.Graphs
{

    public partial class Query
  {

    [GraphQLMetadata("player")]
    public async Task<Player> GetPlayer(string dbname)
    {
      Player player = await playerService.Get(dbname);
      return player; 
    }
  }

  public partial class Mutation
  {

    [GraphQLMetadata("addPlayer")]
    public async Task<CUDMessage> AddPlayer(Player newPlayer)
    {
      if (newPlayer.Games is null) {
        newPlayer.Games = new List<string>() {};
      }

      CUDMessage message = await playerService.Add(newPlayer);
      if (!message.OK) {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newPlayer.DBName}. See log for more details.";
      } else {
        message.Message = $"Successfully added {newPlayer.DBName}.";
      }
      return message; 
    }
  }

}