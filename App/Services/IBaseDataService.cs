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
    Task<T> GetSingle(string uniqueField);
    Task<T> GetSingle(string uniqueField, IViewOption options);
    Task<List<T>> GetList(JsonElement condition);
    Task<List<T>> GetList(JsonElement condition, IViewOption options);
    Task<CUDMessage> AddSingle(T newItem);
    Task<CUDMessage> AddList(List<T> newItems);
    Task<CUDMessage> UpdateSingle(string uniqueField, JsonElement token);
    Task<CUDMessage> UpdateList(JsonElement condition, JsonElement token);
    Task<CUDMessage> DeleteSingle(string uniqueField);
    Task<CUDMessage> DeleteList(JsonElement condition);
  }
}