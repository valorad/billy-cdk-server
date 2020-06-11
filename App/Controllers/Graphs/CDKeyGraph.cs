using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Lib;
using App.Models;
using GraphQL;

namespace App.Controllers.Graphs
{
  public static class CDKeyGraph
  {
    public static List<string> requiredFields = new List<string>() {
      "game",
      "value",
      "platform",
    };

    public static bool HasRequiredFields(CDKey newCDKey)
    {
      // Normally we just need to check string and nullable fields. For non-nullable types,
      // GraphQL will stop the request for us already if the request mutation is invalid
      foreach (var field in requiredFields)
      {
        var value = Property.GetValue(newCDKey, field);
        if (value is null || string.IsNullOrWhiteSpace((string)value))
        {
          return false;
        }
      }

      return true;

    }



  }

  public partial class Query
  {

    [GraphQLMetadata("cdkey")]
    public async Task<CDKey> GetCDKey(string id, string options)
    {

      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
        return await cdkeyService.Get(id, viewOptions);
      }
      else
      {
        return await cdkeyService.Get(id);
      }

    }

    [GraphQLMetadata("cdkeys")]
    public async Task<List<CDKey>> GetCDKeys(string condition, string options)
    {

      JsonElement conditionInJson;

      if (string.IsNullOrWhiteSpace(condition))
      {
        conditionInJson = JsonDocument.Parse("{}").RootElement;
      }
      else
      {
        conditionInJson = JsonDocument.Parse(condition).RootElement;
      }

      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
        return await cdkeyService.Get(conditionInJson, viewOptions);
      }
      else
      {
        return await cdkeyService.Get(conditionInJson);
      }

    }

  }

  public partial class Mutation
  {

    [GraphQLMetadata("addCDKey")]
    public async Task<CUDMessage> AddCDKey(CDKey newCDKey)
    {

      // Check unique fields
      CDKey cdkeyInDB = await cdkeyService.GetByValue(
        newCDKey.Value
      );
      if (cdkeyInDB is { })
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Forbidden to add {newCDKey.Value} because the same value already exists",
        };
      }

      // Check if necessary fields are provided
      bool hasRequiredFields = CDKeyGraph.HasRequiredFields(newCDKey);
      if (!hasRequiredFields)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to add cdkey because at least one required field is not provided. The required fields are: {string.Join(", ", CDKeyGraph.requiredFields)}",
        };
      }

      CUDMessage message = await cdkeyService.Add(newCDKey);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add CDKey {newCDKey.Value}. See log for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newCDKey.Value}.";
      }
      return message;
    }

    [GraphQLMetadata("addCDKeys")]
    public async Task<CUDMessage> AddCDKey(List<CDKey> newCDKeys)
    {

      // stop here if list is empty
      if (newCDKeys.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No cdkeys are added since the add list is empty.",
        };
      }

      // remove conflict cdkeys in the add list (unique field)
      var valueQuoted = newCDKeys.Select((ele) => $"\"{ele.Value}\"");
      string valueInText = string.Join(", ", valueQuoted);

      string conditions = "{"
      + " \"value\": " + "{"
        + "\"$in\": [" + valueInText + "]"
        + "}"
      + "}";
      List<CDKey> conflictCDKeys = await cdkeyService.Get(
        JsonDocument.Parse(conditions).RootElement,
        new DBViewOption()
        {
          Includes = new List<string>() { "value" }
        }
      );

      foreach (var cdkey in conflictCDKeys)
      {
        newCDKeys.Remove(newCDKeys.Find(ele => ele.Value == cdkey.Value));
      }

      // Check if necessary fields are provided
      var incompleteCDKeys = new List<CDKey>() { };
      foreach (var cdkey in newCDKeys)
      {
        if (!CDKeyGraph.HasRequiredFields(cdkey))
        {
          incompleteCDKeys.Add(cdkey);
        }
      }
      // Remove incomplete cdkeys
      foreach (var cdkey in incompleteCDKeys)
      {
        newCDKeys.Remove(newCDKeys.Find(ele => ele.ID == cdkey.ID));
      }

      if (newCDKeys.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No cdkeys to add because of non of these cdkeys are eligible",
        };
      }

      CUDMessage message = await cdkeyService.Add(newCDKeys);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newCDKeys.Count} cdkeys. See logs for more details.";
      }
      else
      {
        message.Message = $"Successfully added {newCDKeys.Count} cdkeys.";
      }
      return message;
    }

    [GraphQLMetadata("updateCDKey")]
    public async Task<CUDMessage> UpdateCDKey(string id, string token)
    {
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await cdkeyService.Update(id, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update CDKey {id}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated CDKey {id}.";
      }

      return message;

    }

    [GraphQLMetadata("updateCDKeys")]
    public async Task<CUDMessage> UpdateCDKeys(string condition, string token)
    {

      if (string.IsNullOrWhiteSpace(condition) || condition.Equals("{}"))
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = "Condition cannot be empty or \"{}\" ",
        };
      }

      JsonElement conditionInJson = JsonDocument.Parse(condition).RootElement;
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await cdkeyService.Update(conditionInJson, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated selected CDKeys.";
      }

      return message;

    }

    [GraphQLMetadata("deleteCDKey")]
    public async Task<CUDMessage> DeleteCDKey(string id)
    {

      CUDMessage message = await cdkeyService.Delete(id);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete CDKey {id}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted {id}.";
      }

      return message;
    }

    [GraphQLMetadata("deleteCDKeys")]
    public async Task<CUDMessage> DeleteCDKeys(string condition)
    {

      if (string.IsNullOrWhiteSpace(condition) || condition.Equals("{}"))
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = "Condition cannot be empty or \"{}\" ",
        };
      }

      JsonElement conditionInJson = JsonDocument.Parse(condition).RootElement;

      CUDMessage message = await cdkeyService.Delete(conditionInJson);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted selected CDKeys.";
      }

      return message;
    }

  }

}