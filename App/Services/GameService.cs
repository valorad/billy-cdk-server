using App.Database;
using App.Models;

namespace App.Services
{
  public class GameService : BaseDataService<Game>, IGameService
  {
    public GameService(IDBCollection collection) : base(collection.Games)
    {
      this.uniqueFieldName = "dbname";
    }
    
  }
}