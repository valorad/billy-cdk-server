using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface ICDKService : IBaseDataService<CDKey>
  {
    Task<CUDMessage> ActivateByDBID(string id);
    Task<CUDMessage> Activate(string value);

    Task<CUDMessage> ActivateByDBID(List<string> ids);
    Task<CUDMessage> Activate(List<string> value);
    Task<CUDMessage> UpdatePlayer(string cdKey, string newPlayer);

  }
}