using BillyCDK.App.Models;
using MongoDB.Driver;

namespace BillyCDK.App.Services;
public interface IAbstractDataService<T>
{
    Task<InstanceMessage<T>> Add(IList<T> newItems);
    Task<InstanceMessage<T>> AddItemsToList(string listFieldName, IEnumerable<string> newValues, FilterDefinition<T> targetCondition);
    Task<InstanceMessage<T>> Delete(FilterDefinition<T> condition);
    Task<InstanceMessage<T>> Drop(FilterDefinition<T> condition);
    Task<IList<T>> Get(FilterDefinition<T> condition, IDBViewOption? options = null);
    Task<InstanceMessage<T>> RemoveItemsFromList(string listFieldName, IEnumerable<string> removingValues, FilterDefinition<T> targetCondition);
    Task<InstanceMessage<T>> Update(FilterDefinition<T> condition, UpdateDefinition<T> token);
}