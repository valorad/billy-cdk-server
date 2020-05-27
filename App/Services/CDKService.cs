using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public class CDKService : BaseDataService<CDKey>, ICDKService
  {
    public CDKService(IDBCollection collection) : base(collection.CDKeys)
    {
      this.uniqueFieldName = "_id";
    }

    public async Task<CDKey> Activate(string value)
    {
      // string condition = "{"
      //   + $"\"value\": \"{value}\" "
      // + "}";

      // string updateToken = "{"
      //   + "\"isActivated\": true "
      // + "}";
      // return await Update(
      //   JsonDocument.Parse(condition).RootElement,
      //   JsonDocument.Parse(updateToken).RootElement
      // );
      throw new System.NotImplementedException();
    }

    public async Task<List<CDKey>> Activate(List<string> value)
    {
      // var valueQuoted = from ele in value select $"\"{ele}\"";
      // string valueInText = string.Join(", ", valueQuoted);
      // string conditions = "{"
      // + " \"value\": " + "{"
      //   + "\"$in\": [" + valueInText + "]"
      //   + "}"
      // + "}";
      // string updateToken = "{"
      //   + "\"isActivated\": true "
      // + "}";
      // return await Update(
      //   JsonDocument.Parse(conditions).RootElement,
      //   JsonDocument.Parse(updateToken).RootElement
      // );
      throw new System.NotImplementedException();
    }

    public async Task<CUDMessage> ActivateByDBID(string id)
    {
      string updateToken = "{"
        + "\"isActivated\": true "
      + "}";
      return await Update(id, JsonDocument.Parse(updateToken).RootElement);
    }

    public async Task<CUDMessage> ActivateByDBID(List<string> ids)
    {
      var idFragments = from ele in ids select $"\"{ele}\"";
      string idFragmentsInText = string.Join(", ", idFragments);
      string conditions = "{"
      + " \"_id\": " + "{"
        + "\"$in\": [" + idFragmentsInText + "]"
        + "}"
      + "}";
      string updateToken = "{"
        + "\"isActivated\": true "
      + "}";
      return await Update(
        JsonDocument.Parse(conditions).RootElement,
        JsonDocument.Parse(updateToken).RootElement
      );

    }

    public async Task<CUDMessage> UpdatePlayer(string cdKey, string newPlayer)
    {
      throw new System.NotImplementedException();
    }
  }
}