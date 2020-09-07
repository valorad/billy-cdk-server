using System.Collections.Generic;

namespace App.Models
{
  public interface IPlayer
  {
    string DBName { get; set; }
    string Name { get; set; }
    string Bio { get; set; }
    bool? IsPremium { get; set; }
    List<string> Games { get; set; }
  }
}