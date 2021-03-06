using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Services
{
  public class BaseDataService<T> : IBaseDataService<T>
  {

    private readonly IMongoCollection<T> collection;
    public string uniqueFieldName { get; set; }

    public BaseDataService(IMongoCollection<T> collection)
    {
      this.collection = collection;
    }

    private FilterDefinition<T> BuildConditions(string uniqueFieldValue)
    {

      FilterDefinition<T> condition;
      if (uniqueFieldName == "_id")
      {
        condition = Builders<T>.Filter.Eq("_id", ObjectId.Parse(uniqueFieldValue));
      }
      else
      {
        condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueFieldValue}\"" + "}";
      }
      return condition;

    }

    public async Task<T> Get(string uniqueFieldValue, IDBViewOption options = null)
    {

      FilterDefinition<T> condition = BuildConditions(uniqueFieldValue);

      var query = collection.Find(condition);

      if (options is { })
      {
        query = View.MakePagination(query, options);
        query = query.Project<T>(View.BuildProjection<T>(options));
        query.Sort(View.BuildSort(options));
      }

      return await query.FirstOrDefaultAsync();
    }

    public async Task<List<T>> Get(JsonElement condition, IDBViewOption options = null)
    {
      FilterDefinition<T> filter = condition.ToString();

      var query = collection.Find(filter);

      if (options is { })
      {
        query = View.MakePagination(query, options);
        query = query.Project<T>(View.BuildProjection<T>(options));
        query.Sort(View.BuildSort(options));
      }

      return await query.ToListAsync();
    }

    public async Task<CUDMessage> Add(T newItem)
    {
      try
      {
        await collection.InsertOneAsync(newItem);
        return new CUDMessage()
        {
          OK = true,
          NumAffected = 1,
          Message = "",
        };
      }
      catch (Exception e)
      {
        return new CUDMessage()
        {
          OK = true,
          NumAffected = 0,
          Message = e.ToString(),
        };
      }
    }

    public async Task<CUDMessage> Add(List<T> newItems)
    {
      try
      {
        await collection.InsertManyAsync(newItems);
        return new CUDMessage()
        {
          OK = true,
          NumAffected = newItems.Count,
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

    public async Task<CUDMessage> Update(string uniqueFieldValue, JsonElement token)
    {

      FilterDefinition<T> condition = BuildConditions(uniqueFieldValue);

      UpdateDefinition<T> updateToken = token.ToString();
      try
      {
        UpdateResult result = await collection.UpdateOneAsync(condition, updateToken);
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

    public async Task<CUDMessage> Update(JsonElement condition, JsonElement token)
    {
      FilterDefinition<T> filter = condition.ToString();
      UpdateDefinition<T> updateToken = token.ToString();
      try
      {
        UpdateResult result = await collection.UpdateManyAsync(filter, updateToken);
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

    public async Task<CUDMessage> Delete(string uniqueFieldValue)
    {

      FilterDefinition<T> condition = BuildConditions(uniqueFieldValue);

      try
      {
        DeleteResult result = await collection.DeleteOneAsync(condition);
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

    public async Task<CUDMessage> Delete(JsonElement condition)
    {
      FilterDefinition<T> filter = condition.ToString();

      try
      {
        DeleteResult result = await collection.DeleteManyAsync(filter);
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
  }
}