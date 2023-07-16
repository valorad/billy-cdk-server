namespace BillyCDK.App.Models;

public record InputCDKey(
    string Value,
    string? Player,
    string Game,
    bool? IsActivated,
    double? Price,
    string? Platform
);

