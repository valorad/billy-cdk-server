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

    public async Task<T> Get(string uniqueField)
    {
      FilterDefinition<T> condition;
      if (uniqueFieldName == "_id") {
        condition = Builders<T>.Filter.Eq("_id", ObjectId.Parse(uniqueField));
      } else {
        condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";
      }

      return await collection.Find(condition).FirstOrDefaultAsync();
    }

    public async Task<T> Get(string uniqueField, IViewOption options)
    {

      FilterDefinition<T> condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";

      var query = collection.Find(condition);

      query = View.MakePagination(query, options);

      query = query.Project<T>(View.BuildProjection<T>(options));

      query.Sort(View.BuildSort(options));

      return await query.FirstOrDefaultAsync();
    }

    public async Task<List<T>> Get(JsonElement condition)
    {
      FilterDefinition<T> filter = condition.ToString();
      return await collection.Find(filter).ToListAsync();
    }

    public async Task<List<T>> Get(JsonElement condition, IViewOption options)
    {
      FilterDefinition<T> filter = condition.ToString();

      var query = collection.Find(filter);

      query = View.MakePagination(query, options);

      query = query.Project<T>(View.BuildProjection<T>(options));

      query.Sort(View.BuildSort(options));

      return await query.ToListAsync();
    }

    public async Task<CUDMessage> Add(T newItem)
    {
      var message = new CUDMessage()
      {
        NumAffected = 1,
        OK = true
      };
      try
      {
        await collection.InsertOneAsync(newItem);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
        message.OK = false;
        message.NumAffected = 0;
      }

      return message;
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

    public async Task<CUDMessage> Update(string uniqueField, JsonElement token)
    {
      FilterDefinition<T> condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";
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

    public async Task<CUDMessage> Delete(string uniqueField)
    {
      FilterDefinition<T> condition = "{" + $" \"{uniqueFieldName}\": " + $"\"{uniqueField}\"" + "}";

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