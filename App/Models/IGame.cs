namespace App.Models
{
  public interface IGame
  {
    string DBName { get; set; }
    string Name { get; set; }
    string Description { get; set; }
    double Price { get; set; }
  }
}