using System.Collections.Generic;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
  public interface ICDKeyService : IBaseDataService<CDKey>
  {
    Task<CDKey> GetByValue(string value, IDBViewOption options = null);
    Task<List<CDKey>> GetByValue(List<string> values, IDBViewOption options = null);
    Task<CUDMessage> ActivateByDBID(string id);
    Task<InstanceCUDMessage<CDKey>> Activate(string playerDBName, string value);
    Task<InstanceCUDMessage<CDKey>> Activate(Player player, string value);
    Task<InstanceCUDMessage<CDKey>> Activate(string playerDBName, List<string> value);
    Task<InstanceCUDMessage<CDKey>> Activate(Player player, List<string> values);
    Task<CUDMessage> ActivateByDBID(List<string> ids);
    
    Task<CUDMessage> UpdatePlayer(string cdKey, string newPlayer);

  }
}