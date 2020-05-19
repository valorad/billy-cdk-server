using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;
using MongoDB.Driver;

namespace App.Services
{
  public class PlayerService : IPlayerService
  {

    private readonly IDBContext context;

    public PlayerService(IDBContext context)
    {
      this.context = context;
    }

    public async Task<CUDMessage> AddPlayer(IPlayer newPlayer)
    {
      var message = new CUDMessage()
      {
        NumAffected = 1,
        OK = true
      };
      try
      {
        await context.Players.InsertOneAsync(newPlayer as Player);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        message.OK = false;
        message.NumAffected = 0;
      }

      return message;
    }

    public async Task<CUDMessage> AddPlayers(List<IPlayer> newPlayers)
    {
      // Lists cannot simply convert via "as".
      List<Player> newPlayersToAdd = newPlayers.ConvertAll(ele => ele as Player);
      try
      {
        await context.Players.InsertManyAsync(newPlayersToAdd);
        // IEnumerable<Player> newPlayersToAdd = newPlayers.AsEnumerable() as IEnumerable<Player>;

        // var d = Ensure.IsNotNull<IEnumerable<Player>>(newPlayersToAdd, nameof(newPlayersToAdd));
        // Ensure.IsNotNull<IEnumerable<Player>>(newPlayers, nameof(newPlayers));
        return new CUDMessage()
        {
          OK = true,
          NumAffected = newPlayers.Count,
          Message = "",
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = e.ToString(),
        };
      }
    }

    public async Task<CUDMessage> DeletePlayer(string dbname)
    {

      try
      {
        DeleteResult result = await context.Players.DeleteOneAsync(p => p.DBName == dbname);
        long numDeleted = result.DeletedCount;
        return new CUDMessage()
        {
          OK = true,
          NumAffected = numDeleted,
          Message = "",
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = e.ToString(),
        };
      }

    }

    public async Task<CUDMessage> DeletePlayers(JsonElement condition)
    {

      FilterDefinition<Player> filter = condition.ToString();

      try
      {
        DeleteResult result = await context.Players.DeleteManyAsync(filter);
        long numDeleted = result.DeletedCount;
        return new CUDMessage()
        {
          OK = true,
          NumAffected = numDeleted,
          Message = "",
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = e.ToString(),
        };
      }
    }

    public async Task<Player> GetPlayer(string dbname)
    {
      return await context.Players.Find(p => p.DBName == dbname).FirstOrDefaultAsync();
    }

    public async Task<Player> GetPlayer(string uniqueField, IViewOption options)
    {

      string uniqueFieldName = "dbname";
      FilterDefinition<Player> condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";

      var query = context.Players.Find(condition);

      query = View.MakePagination(query, options);

      query = query.Project<Player>(View.BuildProjection<Player>(options));

      query.Sort(View.BuildSort(options));

      return await query.FirstOrDefaultAsync();

    }

    public async Task<List<Player>> GetPlayerList(JsonElement condition)
    {
      FilterDefinition<Player> filter = condition.ToString();
      return await context.Players.Find(filter).ToListAsync();
    }

    public async Task<List<Player>> GetPlayerList(JsonElement condition, IViewOption options)
    {
      FilterDefinition<Player> filter = condition.ToString();

      var query = context.Players.Find(filter);

      query = View.MakePagination(query, options);

      query = query.Project<Player>(View.BuildProjection<Player>(options));

      query.Sort(View.BuildSort(options));

      return await query.ToListAsync();

    }

    public async Task<CUDMessage> UpdatePlayer(string uniqueField, JsonElement token)
    {
      string uniqueFieldName = "dbname";
      FilterDefinition<Player> condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";
      UpdateDefinition<Player> updateToken = token.ToString();
      try
      {
        UpdateResult result = await context.Players.UpdateOneAsync(condition, updateToken);
        long numUpdated = result.ModifiedCount;
        return new CUDMessage()
        {
          NumAffected = numUpdated,
          OK = true,
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          Message = e.ToString(),
          NumAffected = 0,
          OK = false,
        };
      }
    }

    public async Task<CUDMessage> UpdatePlayers(JsonElement condition, JsonElement token)
    {
      FilterDefinition<Player> filter = condition.ToString();
      UpdateDefinition<Player> updateToken = token.ToString();
      try
      {
        UpdateResult result = await context.Players.UpdateManyAsync(filter, updateToken);
        long numUpdated = result.ModifiedCount;
        return new CUDMessage()
        {
          NumAffected = numUpdated,
          OK = true,
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          Message = e.ToString(),
          NumAffected = 0,
          OK = false,
        };
      }
    }

  }
}