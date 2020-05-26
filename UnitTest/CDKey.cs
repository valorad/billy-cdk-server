using System;
using System.Collections.Generic;
using App.Database;
using App.Models;
using App.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTest
{
  [Collection("Sequential")]
  public class CDKeyTest : IClassFixture<DbFixture>, IDisposable
  {
    private readonly ServiceProvider serviceProvider;
    private readonly ICDKService cdkService;
    private readonly IDBContext dbContext;

    public CDKeyTest(DbFixture fixture)
    {
      serviceProvider = fixture.ServiceProvider;
      cdkService = serviceProvider.GetService<ICDKService>();
      dbContext = serviceProvider.GetService<IDBContext>();
    }

    public void Dispose()
    {
      // called after each test method
      dbContext.Drop();
    }

    [Theory(DisplayName = "CDkey Activation Test")]
    public async void TestActivate(
      Player player,
      List<Game> games,
      Dictionary<string, List<string>> cdkeys,
      Dictionary<string, List<string>> duplicatedCDKeys
    )
    {
      // prepare player + game

      // create CDK instances for the games according to two maps (at this time: player is null)

      // activate one cdk. Should be able to add, and player then has the game.
      // -> (player not null + appear in "games" field in player insstance) We should get the cdk instance.
      // activate multiple cdks. Should be able to add, and player then has the list of games. We should get a list of cdk instance.
      // check them together after adding cdkeys map

      // activate duplicate keys. Should raise execptions because player already owns the game, activation should fail
      // activate multiple duplicate cdks. No execptions should happen despite duplicated games, and the games will only be added if player does not own those games
      // we should get a list of successfully added cdk instances.
      // "player" field of unsuccessfully added cdkeys should remain null
      // check them seperately when adding duplicatedCDKeys. First check activate one. Then check activate mixed multiple.

    }

  }
}