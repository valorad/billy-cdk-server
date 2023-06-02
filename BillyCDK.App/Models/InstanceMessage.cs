namespace BillyCDK.App.Models;

public record InstanceMessage<T>(
    byte Okay,
    string Message,
    IList<T>? Instances
);
