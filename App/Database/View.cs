using System.Collections.Generic;
using App.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace App.Database
{
  
  public static class View
  {
    public static IFindFluent<T, T> MakePagination<T>(IFindFluent<T, T> query, IPaginationOption options)
    {
      // Pagination
      int pageSize = (options.PerPage > 0 ? options.PerPage : 10);
      int currentPage = (options.Page > 0 ? options.Page : 1);
      return query.Limit(pageSize).Skip((currentPage - 1) * pageSize);
    }

    public static ProjectionDefinition<T> BuildProjection<T>(IProjectionOption options)
    {
      // Projection
      var projectionToken = new Dictionary<string, int>() { };

      if (options.Includes is {}) {
        foreach (var field in options.Includes)
        {
          projectionToken.Add(field, 1);
        }
      }

      if (options.Excludes is {}) {
        foreach (var field in options.Excludes)
        {
          projectionToken.Add(field, 0);
        }
      }

      return projectionToken.ToJson();

    }

    public static string BuildSort(ISortOption options)
    {
      // Sorting
      if (options.OrderBy != null)
      {
        return $"{{ {options.OrderBy}: { (options.Order == "desc" ? -1 : 1) } }}";
      }
      return "{}";
    }

  }
}