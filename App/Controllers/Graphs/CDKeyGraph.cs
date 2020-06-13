using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Lib;
using App.Models;
using App.Services;
using GraphQL;

namespace App.Controllers.Graphs
{
  public class CDKeyGraph : RootGraph<CDKey>
  {
    private readonly ICDKeyService cdkeyService;
    private readonly IPlayerService playerService;

    public CDKeyGraph(
      ICDKeyService cdkeyService,
      IPlayerService playerService
    ) : base(cdkeyService)
    {
      this.itemName = "CDKey";

      // this.defaultField = "ID"; // is by default, thus no need to specify

      this.uniqueFields = new List<string>() {
        "value",
      };

      this.requiredFields = new List<string>() {
        "game",
        "value",
        "platform",
      };

      this.cdkeyService = cdkeyService;
      this.playerService = playerService;

    }

    public async Task<CDKey> GetCDKeyByValue(string cdkeyValue, string options)
    {
      DBViewOption viewOptions = null;
      if (options is { })
      {
        JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
        viewOptions = optionsInJson.JSONToObject<DBViewOption>();
      }
      return await cdkeyService.GetByValue(cdkeyValue, viewOptions);
    }

    public async Task<InstanceCUDMessage<CDKey>> ActivateCDKey(string playerDBName, string value)
    {
      // check player existence
      CDKey cdkey;
      try
      {
        cdkey = await cdkeyService.Activate(playerDBName, value);
        return new InstanceCUDMessage<CDKey>()
        {
          OK = true,
          NumAffected = 1,
          Message = $"Successfully activated CDKey with value = {value} for player with dbname = {playerDBName}",
          Instance = cdkey,
        };
      }
      catch (Exception e)
      {
        return new InstanceCUDMessage<CDKey>()
        {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to activate CDKey with value = {value}: {e.Message}",
          Instance = null,
        };
      }
    }

    public async Task<InstanceCUDMessage<CDKey>> ActivateCDKey(string playerDBName, List<string> values)
    {

      // check if cdkey list is empty
      if (values.Count <= 0) {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"No CDKeys to activate since the input list is empty.",
          Instance = null,
        };
      }

      // check if player exist
      Player player = await playerService.Get(playerDBName);
      if (player is null) {
        return new InstanceCUDMessage<CDKey>() {
          OK = false,
          NumAffected = 0,
          Message = $"Failed to activate CDKey these cdkeys: Player {playerDBName} deos not exist.",
          Instance = null,
        };
      }

      // begin activate CDKeys
      var cdkeys = await cdkeyService.Activate(playerDBName, values);
      return new InstanceCUDMessage<CDKey>()
      {
        OK = true,
        NumAffected = 1,
        Message = $"Successfully activated specified cdkeys for player with dbname = {playerDBName}",
        Instances = cdkeys,
      };

    }

    public async Task<CDKey> RerouteSingleQuery(CDKeyQueryParameters cdkeyQueryParameters)
    {
      if (cdkeyQueryParameters.ID is { })
      {
        return await GetSingle(cdkeyQueryParameters.ID, cdkeyQueryParameters.Options);
      }
      if (cdkeyQueryParameters.Value is { })
      {
        return await GetCDKeyByValue(cdkeyQueryParameters.Value, cdkeyQueryParameters.Options);
      }
      return null;
    }


  }

  public class InstanceCUDMessage<T> : CUDMessage
  {
    public T Instance { get; set; }
    public List<T> Instances { get; set; }
  }

  public partial class Query
  {

    [GraphQLMetadata("cdkey")]
    public async Task<CDKey> GetCDKey(CDKeyQueryParameters parameters)
    {
      return await cdkeyGraph.RerouteSingleQuery(parameters);
    }

    [GraphQLMetadata("cdkeys")]
    public async Task<List<CDKey>> GetCDKeys(string condition, string options)
    {
      return await cdkeyGraph.GetList(condition, options);
    }

  }

  public partial class Mutation
  {

    #region Basic CUD
    [GraphQLMetadata("addCDKey")]
    public async Task<CUDMessage> AddCDKey(CDKey newCDKey)
    {
      return await cdkeyGraph.AddSingle(newCDKey);
    }

    [GraphQLMetadata("addCDKeys")]
    public async Task<CUDMessage> AddCDKey(List<CDKey> newCDKeys)
    {
      return await cdkeyGraph.AddList(newCDKeys);
    }

    [GraphQLMetadata("updateCDKey")]
    public async Task<CUDMessage> UpdateCDKey(string id, string token)
    {
      return await cdkeyGraph.UpdateSingle(id, token);
    }

    [GraphQLMetadata("updateCDKeys")]
    public async Task<CUDMessage> UpdateCDKeys(string condition, string token)
    {
      return await cdkeyGraph.UpdateList(condition, token);
    }

    [GraphQLMetadata("deleteCDKey")]
    public async Task<CUDMessage> DeleteCDKey(string id)
    {
      return await cdkeyGraph.DeleteSingle(id);
    }

    [GraphQLMetadata("deleteCDKeys")]
    public async Task<CUDMessage> DeleteCDKeys(string condition)
    {
      return await cdkeyGraph.DeleteList(condition);
    }
    #endregion

    [GraphQLMetadata("activateCDKey")]
    public async Task<InstanceCUDMessage<CDKey>> ActivateCDKey(string playerDBName, string value)
    {
      return await cdkeyGraph.ActivateCDKey(playerDBName, value);
    }

    [GraphQLMetadata("activateCDKeys")]
    public async Task<InstanceCUDMessage<CDKey>> ActivateCDKey(string playerDBName, List<string> values)
    {
      return await cdkeyGraph.ActivateCDKey(playerDBName, values);
    }

  }

}