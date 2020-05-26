using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface ICDKService : IBaseDataService<CDKey>
  {
    Task<CUDMessage> ActivateByDBID(string id);
    Task<CDKey> Activate(string value);
    Task<CUDMessage> ActivateByDBID(List<string> ids);
    Task<List<CDKey>> Activate(List<string> value);
    Task<CUDMessage> UpdatePlayer(string cdKey, string newPlayer);

  }
}