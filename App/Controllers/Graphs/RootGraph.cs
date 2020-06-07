using App.Services;

namespace App.Controllers.Graphs
{

  public partial class Query
  {
    private readonly IPlayerService playerService; 
    private readonly IGameService gameService;
    private readonly ICDKService cdkService;

    public Query(
      IPlayerService playerService,
      IGameService gameService,
      ICDKService cdkService
    )
    {
      this.playerService = playerService;
      this.gameService = gameService;
      this.cdkService = cdkService;
    }
  }

  public partial class Mutation
  {
    private readonly IPlayerService playerService; 
    private readonly IGameService gameService;
    private readonly ICDKService cdkService;

    public Mutation(
      IPlayerService playerService,
      IGameService gameService,
      ICDKService cdkService
    )
    {
      this.playerService = playerService;
      this.gameService = gameService;
      this.cdkService = cdkService;
    }
  }

}