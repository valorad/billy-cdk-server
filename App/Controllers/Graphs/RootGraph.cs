using App.Services;

namespace App.Controllers.Graphs
{

  public partial class Query
  {
    private readonly IPlayerService playerService; 
    private readonly IGameService gameService;
    private readonly ICDKeyService cdkeyService;

    public Query(
      IPlayerService playerService,
      IGameService gameService,
      ICDKeyService cdkeyService
    )
    {
      this.playerService = playerService;
      this.gameService = gameService;
      this.cdkeyService = cdkeyService;
    }
  }

  public partial class Mutation
  {
    private readonly IPlayerService playerService; 
    private readonly IGameService gameService;
    private readonly ICDKeyService cdkeyService;

    public Mutation(
      IPlayerService playerService,
      IGameService gameService,
      ICDKeyService cdkeyService
    )
    {
      this.playerService = playerService;
      this.gameService = gameService;
      this.cdkeyService = cdkeyService;
    }
  }

}