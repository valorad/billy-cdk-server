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

    public CDKeyGraph(ICDKeyService cdkeyService) : base(cdkeyService)
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

    }

    public async Task<CDKey> GetCDKeyByValue(string cdkeyValue, string options)
    {

      JsonElement optionsInJson = JsonDocument.Parse(options).RootElement;
      var viewOptions = optionsInJson.JSONToObject<DBViewOption>();
      return await cdkeyService.GetByValue(cdkeyValue, viewOptions);

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

  }

}