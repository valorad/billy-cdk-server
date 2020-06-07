using System.Collections.Generic;

namespace App.Models
{

  public interface IProjectionOption
  {
    List<string> Includes { get; set; }
    List<string> Excludes { get; set; }
  }

  public interface IPaginationOption
  {
    int Page { get; set; }
    int PerPage { get; set; }
  }

  public interface ISortOption
  {
    string OrderBy { get; set; }
    string Order { get; set; }
  }

  public interface IDBViewOption : IProjectionOption, IPaginationOption, ISortOption { }

}