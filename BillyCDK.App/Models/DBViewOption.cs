namespace BillyCDK.App.Models;

public record DBViewOption(
    List<string>? Includes,
    List<string>? Excludes,
    int Page,
    int PerPage,
    string? OrderBy,
    string? Order
) : IDBViewOption;
