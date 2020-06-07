using System.Collections.Generic;

namespace App.Models
{
  public class DBViewOption : IDBViewOption
  {
    public List<string> Includes { get; set; }
    public List<string> Excludes { get; set; }
    public int Page { get; set; }
    public int PerPage { get; set; }
    public string OrderBy { get; set; }
    public string Order { get; set; }
  }
}