namespace BillyCDK.DataAccess.Models;

public record InputGame(
    string? DBName,
    string? Name,
    string? Description,
    decimal? Price
)
{
    public InputGame() : this("", default, default, default) { }
};
