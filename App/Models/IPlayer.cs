using System.Collections.Generic;

namespace App.Models
{
  public interface IPlayer
  {
    string DBName { get; set; }
    bool IsPremium { get; set; }
    List<string> CDKeys { get; set; }
    List<string> Games { get; set; }
  }
}