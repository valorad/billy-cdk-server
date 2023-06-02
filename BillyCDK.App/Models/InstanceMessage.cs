namespace BillyCDK.App.Models;

public record InstanceMessage<T>(
    byte Okay,
    string Message,
    long NumAffected,
    IList<T>? Instances
);
