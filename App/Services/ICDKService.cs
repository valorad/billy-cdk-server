using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface ICDKService : IBaseDataService<CDKey>
  {
    Task<CDKey> GetByValue(string value);
    Task<List<CDKey>> GetByValue(List<string> values);
    Task<CUDMessage> ActivateByDBID(string id);
    Task<CDKey> Activate(string player, string value);
    Task<CUDMessage> ActivateByDBID(List<string> ids);
    Task<List<CDKey>> Activate(string player, List<string> value);
    Task<CUDMessage> UpdatePlayer(string cdKey, string newPlayer);

  }
}