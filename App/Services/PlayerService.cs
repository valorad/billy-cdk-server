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

    public Task<CUDMessage> AddPlayers(List<IPlayer> newPlayers)
    {
      throw new NotImplementedException();
    }

    public Task<CUDMessage> DeletePlayer(string dbname)
    {
      throw new System.NotImplementedException();
    }

    public Task<CUDMessage> DeletePlayers(JsonElement condition)
    {
      throw new NotImplementedException();
    }

    public async Task<Player> GetPlayer(string dbname)
    {
      return await context.Players.Find(p => p.DBName == dbname).FirstOrDefaultAsync();
    }

    public Task<Player> GetPlayer(string dbname, IViewOption options)
    {
      throw new NotImplementedException();
    }

    public async Task<List<Player>> GetPlayerList(JsonElement condition)
    {
            FilterDefinition<Player> filter = condition.ToString();
            return await context.Players.Find(filter).ToListAsync();
    }

    public Task<List<Player>> GetPlayerList(JsonElement condition, IViewOption options)
    {
      throw new System.NotImplementedException();
    }

    public Task<CUDMessage> UpdatePlayer(string uniqueField, JsonElement token)
    {
      throw new NotImplementedException();
    }

    public Task<CUDMessage> UpdatePlayers(JsonElement condition, JsonElement token)
    {
      throw new System.NotImplementedException();
    }
  }
}