using System.Collections.Generic;

namespace App.Models
{
  public interface ICDKey
  {
    string Player { get; set; }
    string Game { get; set; }
    string Value { get; set; }
    bool IsActivated { get; set; }
    double Price { get; set; }
    string Platform { get; set; }

  }
}