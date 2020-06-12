using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using App.Database;
using App.Models;

namespace App.Services
{
  public interface IBaseDataService<T>
  {
    string uniqueFieldName { get; set; }
    Task<T> Get(string uniqueField, IDBViewOption options = null);
    Task<List<T>> Get(JsonElement condition, IDBViewOption options = null);
    Task<CUDMessage> Add(T newItem);
    Task<CUDMessage> Add(List<T> newItems);
    Task<CUDMessage> Update(string uniqueField, JsonElement token);
    Task<CUDMessage> Update(JsonElement condition, JsonElement token);
    Task<CUDMessage> Delete(string uniqueField);
    Task<CUDMessage> Delete(JsonElement condition);
  }
}