using BillyCDK.App.Models;


namespace BillyCDK.App.Services;

public class PlayerService
{
    public Task<List<Player>> Get()
    {
        return Task.FromResult( new List<Player>()
        {
            new Player(
                ID: "abc",
                DBName: "abc",
                Name: "abc",
                Bio: "abc",
                IsPremium: false,
                Games: new List<string> { "www" }
            ),
        });
    }
}