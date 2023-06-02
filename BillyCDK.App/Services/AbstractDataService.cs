using BillyCDK.App.Models;
using BillyCDK.App.Utilities;
using MongoDB.Driver;

namespace BillyCDK.App.Services;

public abstract class AbstractDataService<T>
{
    protected abstract IMongoCollection<T> Collection { get; set; }
    protected abstract string EntityName { get; set; }

    public async Task<List<T>> Get(FilterDefinition<T> condition, IDBViewOption? options = null)
    {
        var query = Collection.Find(condition);

        if (options is { })
        {
            query = DBUtils.MakePagination(query, options);
            query = query.Project<T>(DBUtils.BuildProjection(options));
            query.Sort(DBUtils.BuildSort(options));
        }

        return await query.ToListAsync();
    }


    public async Task<InstanceMessage<T>> Add(IList<T> newItems)
    {
        try
        {
            await Collection.InsertManyAsync(newItems);

            List<T> insertedEntities = await Get(
                "{}",
                new DBViewOption(
                    Includes: new List<string> { "_id", "dbname" },
                    Excludes: null,
                    Page: 1,
                    PerPage: newItems.Count,
                    OrderBy: "_id",
                    Order: "desc"
                )
            );

            return new InstanceMessage<T>(
                Okay: 1,
                NumAffected: insertedEntities.Count,
                Message: $"Added {EntityName} x{insertedEntities.Count}",
                Instances: insertedEntities
            );
        }
        catch (Exception e)
        {
            return new InstanceMessage<T>(
                Okay: 0,
                NumAffected: 0,
                Message: e.Message,
                Instances: null
            );
        }
    }

    public async Task<InstanceMessage<T>> Update(FilterDefinition<T> condition, UpdateDefinition<T> token)
    {
        try
        {
            UpdateResult result = await Collection.UpdateManyAsync(condition, token);
            List<T> updatedEntities = await Get(
                condition,
                new DBViewOption(
                    Includes: new List<string> { "_id", "dbname" },
                    Excludes: null,
                    Page: 1,
                    PerPage: (int) result.ModifiedCount,
                    OrderBy: "_id",
                    Order: "asc"
                )
            );

            return new InstanceMessage<T>(
                Okay: 1,
                NumAffected: result.ModifiedCount,
                Message: $"Updated {EntityName} x{result.ModifiedCount}",
                Instances: updatedEntities
            );

        }
        catch (Exception e)
        {
            return new InstanceMessage<T>(
                Okay: 0,
                NumAffected: 0,
                Message: e.Message,
                Instances: null
            );
        }
    }

    public async Task<InstanceMessage<T>> Delete(FilterDefinition<T> condition)
      => await Update(
        condition,
        Builders<T>.Update.Set("deletedDate", DateTime.Now)
    );
    

    public async Task<InstanceMessage<T>> Drop(FilterDefinition<T> condition)
    {
        try
        {
            List<T> droppingEntities = await Get(
                condition,
                new DBViewOption(
                    Includes: new List<string> { "_id", "dbname" },
                    Excludes: null,
                    Page: 1,
                    PerPage: 1000,
                    OrderBy: "_id",
                    Order: "asc"
                )
            );
            DeleteResult result = await Collection.DeleteManyAsync(condition);
            return new InstanceMessage<T>(
                Okay: 1,
                NumAffected: droppingEntities.Count,
                Message: $"Dropped {EntityName} x{droppingEntities.Count}",
                Instances: droppingEntities
            );

        }
        catch (Exception e)
        {
            return new InstanceMessage<T>(
                Okay: 0,
                NumAffected: 0,
                Message: e.Message,
                Instances: null
            );
        }

    }

}
