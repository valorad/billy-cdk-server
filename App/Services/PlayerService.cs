using App.Database;
using App.Models;

namespace App.Services
{
  public class PlayerService : BaseDataService<Player>, IPlayerService
  {
    public PlayerService(IDBCollection collection) : base(collection.Players)
    {
      this.uniqueFieldName = "dbname";
    }

  }

}