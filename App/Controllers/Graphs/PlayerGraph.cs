using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Services;
using GraphQL;

namespace App.Controllers.Graphs
{
  public class PlayerGraph : RootGraph<Player>
  {
    private readonly IPlayerService playerService;

    public PlayerGraph(IPlayerService playerService) : base(playerService)
    {

      this.itemName = "player";

      this.defaultField = "dbname";

      this.uniqueFields = new List<string>() {
        "dbname"
      };

      this.requiredFields = new List<string>() {
        "dbname"
      };

      this.playerService = playerService;
    }

    // specify the default values to fill
    public override void Polyfill(Player newPlayer)
    {

      if (newPlayer.Games is null)
      {
        newPlayer.Games = new List<string>() { };
      }

    }

    public async Task<CUDMessage> AddGame(string playerName, string gameName)
    {

      // check if player exists
      Player player = await playerService.Get(playerName);

      if (player is null)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Player {playerName} does not exist.",
        };
      }

      // check if player has already owned the game
      if (player.Games.Exists(ele => ele == gameName))
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Player {player.DBName} already owns the Game {gameName}.",
        };
      }

      // begin adding the game to player
      CUDMessage message = await playerService.AddGame(playerName, gameName);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add game {gameName} to player {playerName}. See logs for details.";
      }
      else
      {
        message.Message = $"Successfully added game {gameName} to player {playerName}.";
      }
      return message;
    }

    public async Task<CUDMessage> AddGame(string playerName, List<string> gameNames)
    {

      int gameInputNum = gameNames.Count;

      // stop here if list is empty
      if (gameInputNum <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No games are added to player {playerName} since the input list is empty.",
        };
      }

      // check if player exists
      Player player = await playerService.Get(playerName);

      if (player is null)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Player {playerName} does not exist.",
        };
      }

      // remove duplicate game entries
      gameNames = gameNames.Distinct().ToList();

      // remove games that are already owned
      RemovePlayerAddGameConflict(player, gameNames);
      // stop here if no valid games
      if (gameNames.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No games can be added to player {player.DBName} because non of these inputs are valid. Try add them one by one if you are not sure why.",
        };
      }

      // begin adding these games to player
      CUDMessage message = await playerService.AddGame(player, gameNames);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {gameNames.Count} games to player {player.DBName}. See logs for details.";
      }
      else
      {
        message.Message = $"Adding games to player {player.DBName}: {gameNames.Count} success, {gameInputNum - gameNames.Count } failure.";
      }
      return message;
    }

    private void RemovePlayerAddGameConflict(Player player, List<string> gameNames)
    {

      var conflictGames = new List<string>() { };
      foreach (var game in gameNames)
      {
        if (player.Games.Exists(ele => ele == game))
        {
          conflictGames.Add(game);
        }
      }

      foreach (var game in conflictGames)
      {
        gameNames.Remove(gameNames.Find(ele => ele == game));
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

    #region Basic CUD
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

    #endregion

    [GraphQLMetadata("playerAddGame")]
    public async Task<CUDMessage> AddGame(string player, string game)
    {
      return await playerGraph.AddGame(player, game);
    }

    [GraphQLMetadata("playerAddGames")]
    public async Task<CUDMessage> AddGames(string player, List<string> games)
    {
      return await playerGraph.AddGame(player, games);
    }

  }

}