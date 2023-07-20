using BillyCDK.DataAccess.Models;

namespace BillyCDK.DataAccess.Services;

public interface ICDKeyService : IAbstractDataService<CDKey>
{
    Task<InstanceMessage<CDKey>> Activate(Player player, IList<string> values);
    Task<InstanceMessage<CDKey>> Activate(string playerDBName, IList<string> values);
    Task<InstanceMessage<CDKey>> AddCDKeys(IList<InputCDKey> newCDKeys);
    Task<IList<CDKey>> GetByValues(IList<string> values, IDBViewOption? options = null);
    Task<InstanceMessage<CDKey>> UpdateIsActivated(IList<string> ids, bool activate = true);
    Task<InstanceMessage<CDKey>> UpdateAsActivated(IDictionary<string, string> cdkeyIDToPlayerNameMap, bool activate = true);
    Task<InstanceMessage<CDKey>> UpdatePlayerByValue(string cdkValue, string newPlayerName);
}