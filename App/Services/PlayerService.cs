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

    public Task<CUDMessage> DeletePlayer(string dbname)
    {
      throw new System.NotImplementedException();
    }

    public async Task<Player> GetPlayer(string dbname)
    {
      return await context.Players.Find(p => p.DBName == dbname).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Player>> GetPlayerList(JsonElement condition)
    {
      throw new System.NotImplementedException();
    }

    public Task<IEnumerable<Player>> GetPlayerList(JsonElement condition, View options)
    {
      throw new System.NotImplementedException();
    }

    public Task<CUDMessage> UpdatePlayers(JsonElement condition, JsonElement token)
    {
      throw new System.NotImplementedException();
    }
  }
}