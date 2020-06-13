using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Lib;
using App.Models;
using App.Services;

namespace App.Controllers.Graphs
{

  public class RootGraph<T>
  {
    public string itemName = "item";
    public string defaultField = "ID";
    public List<string> requiredFields = new List<string>() { }; // Usually the string fields that should not be empty. e.g. dbname
    public List<string> uniqueFields = new List<string>() { }; // Fields (except "_id") that are required to be unique
    private readonly IBaseDataService<T> baseDataService;

    public RootGraph(
      IBaseDataService<T> baseDataService
    )
    {
      this.baseDataService = baseDataService;
    }

    public async Task<T> GetSingle(string defaultFieldValue, string options)
    {
      DBViewOption viewOptions = null;
      if (options is {}) {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        viewOptions = optionsInJson.JSONToObject<DBViewOption>();
      }
      return await baseDataService.Get(defaultFieldValue, viewOptions);
    }

    public async Task<List<T>> GetList(string condition, string options)
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

      DBViewOption viewOptions = null;
      if (options is {}) {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        viewOptions = optionsInJson.JSONToObject<DBViewOption>();
      }

      return await baseDataService.Get(conditionInJson, viewOptions);
      
    }

    public async Task<CUDMessage> AddSingle(T newItem)
    {

      // Check if necessary fields are provided
      FieldInspectionMessage completenessMessage = CheckNewItemComplete(newItem);
      if (!completenessMessage.IsPassed)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to add the {itemName} because {completenessMessage.FailedField} (or more fields) is not provided. The Required fields: {string.Join(", ", requiredFields)}",
        };
      }

      // Check unique
      FieldInspectionMessage uniquenessMessage = await CheckNewItemUnique(newItem);
      if (!uniquenessMessage.IsPassed)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Conflict detected when adding the {itemName} with {uniquenessMessage.FailedField} = {Property.GetValue(newItem, uniquenessMessage.FailedField)}",
        };
      }

      // Polyfill the less important fields
      Polyfill(newItem);

      // Begin inserting to the database
      CUDMessage message = await baseDataService.Add(newItem);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {itemName} with {defaultField} = {Property.GetValue(newItem, defaultField)}. See log for more details.";
      }
      else
      {
        message.Message = $"Successfully added {itemName} with {defaultField} = {Property.GetValue(newItem, defaultField)}.";
      }
      return message;
    }

    public async Task<CUDMessage> AddList(List<T> newItems)
    {
      int inputNum = newItems.Count;

      // stop here if list is empty
      if (inputNum <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No {itemName}(s) are added since the input list is empty.",
        };
      }

      // Remove conflict items in the add list
      await RemoveConflictItems(newItems);

      // Check if necessary fields are provided
      RemoveIncompleteItems(newItems);

      // stop here if list is empty after filtering
      if (newItems.Count <= 0)
      {
        return new CUDMessage()
        {
          OK = false,
          NumAffected = 0,
          Message = $"No {itemName} to add because non of these inputs are valid. Try add them one by one if you are not sure why.",
        };
      }

      // Polyfill with default values
      Polyfill(newItems);

      // Begin inserting into database
      CUDMessage message = await baseDataService.Add(newItems);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to add {newItems.Count} {itemName}(s). See logs for more details.";
      }
      else
      {
        message.Message = $"Add {itemName}(s): {newItems.Count} success, {inputNum - newItems.Count} failure.";
      }
      return message;
    }

    public async Task<CUDMessage> UpdateSingle(string defaultFieldValue, string token)
    {
      JsonElement updateToken = JsonDocument.Parse(token).RootElement;

      CUDMessage message = await baseDataService.Update(defaultFieldValue, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update the {itemName} with {defaultField} = {defaultFieldValue}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated {itemName} with {defaultField} = {defaultFieldValue}.";
      }

      return message;

    }

    public async Task<CUDMessage> UpdateList(string condition, string token)
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

      CUDMessage message = await baseDataService.Update(conditionInJson, updateToken);

      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to update the specified {itemName}(s). See logs for details";
      }
      else
      {
        message.Message = $"Successfully updated the selected {itemName}(s).";
      }

      return message;

    }

    public async Task<CUDMessage> DeleteSingle(string defaultFieldValue)
    {

      CUDMessage message = await baseDataService.Delete(defaultFieldValue);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete the {itemName} with {defaultField} = {defaultFieldValue}. See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted the {itemName} with {defaultField} = {defaultFieldValue}.";
      }

      return message;
    }

    public async Task<CUDMessage> DeleteList(string condition)
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

      CUDMessage message = await baseDataService.Delete(conditionInJson);
      if (!message.OK)
      {
        Console.WriteLine(message.Message);
        message.Message = $"Failed to delete the specified {itemName}(s). See logs for details";
      }
      else
      {
        message.Message = $"Successfully deleted selected {itemName}(s).";
      }

      return message;
    }

    public virtual void Polyfill(T newItem) { }

    public void Polyfill(List<T> newItems)
    {
      foreach (var item in newItems)
      {
        Polyfill(item);
      }
    }

    private async Task<FieldInspectionMessage> CheckNewItemUnique(T newItem)
    {

      foreach (var fieldName in uniqueFields)
      {
        string condition = "{"
        + $" \"{fieldName}\": \"{Property.GetValue(newItem, fieldName)}\" "
        + "}";

        var itemGroup = await baseDataService.Get(
          JsonDocument.Parse(condition).RootElement,
          new DBViewOption()
          {
            Includes = new List<string>() { fieldName }
          }
        );

        T itemInDB = itemGroup.ElementAtOrDefault(0); // safe access position 0

        if (itemInDB is { })
        {
          return new FieldInspectionMessage()
          {
            IsPassed = false,
            FailedField = fieldName
          };
        }
      }

      return new FieldInspectionMessage()
      {
        IsPassed = true,
        FailedField = null
      };

    }

    private FieldInspectionMessage CheckNewItemComplete(T newItem)
    {

      foreach (var field in requiredFields)
      {
        var value = Property.GetValue(newItem, field);
        if (value is null || string.IsNullOrWhiteSpace((string)value))
        {
          return new FieldInspectionMessage()
          {
            IsPassed = false,
            FailedField = field
          };
        }
      }

      return new FieldInspectionMessage()
      {
        IsPassed = true,
        FailedField = null
      };
    }

    private async Task RemoveConflictItems(List<T> newItems)
    {
      var conflictItems = new List<T>() { };
      foreach (var item in newItems)
      {
        FieldInspectionMessage uniquenessMessage = await CheckNewItemUnique(item);
        if (!uniquenessMessage.IsPassed)
        {
          conflictItems.Add(item);
        }
      }

      foreach (var item in conflictItems)
      {
        newItems.Remove(newItems.Find(ele => Property.GetValue(ele, defaultField) == Property.GetValue(item, defaultField)));
      }

    }

    private void RemoveIncompleteItems(List<T> newItems)
    {
      var incompleteItems = new List<T>() { };
      foreach (var item in newItems)
      {
        FieldInspectionMessage completenessMessage = CheckNewItemComplete(item);
        if (!completenessMessage.IsPassed)
        {
          incompleteItems.Add(item);
        }
      }
      // Remove incomplete players
      foreach (var item in incompleteItems)
      {
        newItems.Remove(newItems.Find(ele => Property.GetValue(ele, defaultField) == Property.GetValue(item, defaultField)));
      }

    }

  }


  public partial class Query
  {
    private readonly PlayerGraph playerGraph;
    private readonly GameGraph gameGraph;
    private readonly CDKeyGraph cdkeyGraph;

    public Query(
      PlayerGraph playerGraph,
      GameGraph gameGraph,
      CDKeyGraph cdkeyGraph
    )
    {
      this.playerGraph = playerGraph;
      this.gameGraph = gameGraph;
      this.cdkeyGraph = cdkeyGraph;
    }
  }

  public partial class Mutation
  {
    private readonly PlayerGraph playerGraph;
    private readonly GameGraph gameGraph;
    private readonly CDKeyGraph cdkeyGraph;

    public Mutation(
      PlayerGraph playerGraph,
      GameGraph gameGraph,
      CDKeyGraph cdkeyGraph
    )
    {
      this.playerGraph = playerGraph;
      this.gameGraph = gameGraph;
      this.cdkeyGraph = cdkeyGraph;
    }
  }

}