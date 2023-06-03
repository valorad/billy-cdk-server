using BillyCDK.App.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BillyCDK.App.Utilities;

public class DBUtils
{
    public static IFindFluent<T, T> MakePagination<T>(IFindFluent<T, T> query, IPaginationOption options)
    {
        var pageSize = (options.PerPage > 0 ? options.PerPage : 10);
        var currentPage = (options.Page > 0 ? options.Page : 1);
        return query.Limit(pageSize).Skip((currentPage - 1) * pageSize);
    }

    public static string BuildProjection(IProjectionOption options)
    {
        var projectionToken = new Dictionary<string, int>() { };

        if (options.Includes is { })
        {
            foreach (var field in options.Includes)
            {
                projectionToken.Add(field, 1);
            }
        }

        if (options.Excludes is { })
        {
            foreach (var field in options.Excludes)
            {
                projectionToken.Add(field, 0);
            }
        }

        return projectionToken.ToJson();

    }

    public static string BuildSort(ISortOption options)
    {
        if (options.OrderBy is { })
        {
            return JsonUtils.CreateCompactLiteral($@"{{
                {options.OrderBy}: {(options.Order == "desc" ? -1 : 1)}
            }}");
        }
        return "{}";
    }

    public static FilterDefinition<T> BuildSingleQueryCondition<T>(string indexFieldName, string indexFieldValue)
    {

        FilterDefinition<T> condition;
        if (indexFieldName == "_id")
        {
            condition = Builders<T>.Filter.Eq(indexFieldName, ObjectId.Parse(indexFieldValue));
        }
        else
        {
            condition = Builders<T>.Filter.Eq(indexFieldName, indexFieldValue);
        }
        return condition;
    }

    public static void AddFilterToPipeline(IList<BsonDocument> pipelineStages, string conditionLiteral)
    {
        pipelineStages.Add(BsonDocument.Parse(JsonUtils.CreateCompactLiteral($@"{{
            ""$match"": {conditionLiteral}
        }}")));
    }

    public static void AddPaginationToPipeline(IList<BsonDocument> pipelineStages, IPaginationOption options)
    {
        var pageSize = (options.PerPage > 0 ? options.PerPage : 10);
        var currentPage = (options.Page > 0 ? options.Page : 1);

        pipelineStages.Add(BsonDocument.Parse(JsonUtils.CreateCompactLiteral($@"{{
            ""$limit"": {pageSize}
        }}")));

        pipelineStages.Add(BsonDocument.Parse(JsonUtils.CreateCompactLiteral($@"{{
            ""$skip"": {(currentPage - 1) * pageSize}
        }}")));

    }

    public static void AddProjectionToPipeline(IList<BsonDocument> pipelineStages, IProjectionOption options)
    {
        pipelineStages.Add(BsonDocument.Parse(JsonUtils.CreateCompactLiteral($@"{{
            ""$project"": {BuildProjection(options)}
        }}")));
    }

    public static void AddSortToPipeline(IList<BsonDocument> pipelineStages, ISortOption options)
    {
        pipelineStages.Add(BsonDocument.Parse(JsonUtils.CreateCompactLiteral($@"{{
            ""$sort"": {BuildSort(options)}
        }}")));
    }
}
