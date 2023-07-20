namespace BillyCDK.DataAccess.Models;

public record InputPlayer(
    string DBName,
    string? Name,
    string? Bio,
    bool? IsPremium,
    List<string>? Games
)
{
    public InputPlayer() : this("", default, default, default, default) { }
};    

