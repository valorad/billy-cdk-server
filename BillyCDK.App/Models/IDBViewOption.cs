namespace BillyCDK.App.Models;


public interface IProjectionOption
{
    List<string>? Includes { get; init; }
    List<string>? Excludes { get; init; }
}

public interface IPaginationOption
{
    int Page { get; init; }
    int PerPage { get; init; }
}

public interface ISortOption
{
    string? OrderBy { get; init; }
    string? Order { get; init; }
}

public interface IDBViewOption : IProjectionOption, IPaginationOption, ISortOption { }
