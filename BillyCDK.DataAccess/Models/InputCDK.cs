namespace BillyCDK.DataAccess.Models;

public record InputCDKey(
    string Value,
    string? Player,
    string Game,
    bool? IsActivated,
    double? Price,
    string? Platform
);

