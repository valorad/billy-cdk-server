namespace BillyCDK.App.Utilities;

public static class EnumerableExtension
{
    /// <summary>
    /// Surround a string with two markers, and
    /// separate the list of strings with a given separator
    /// e.g. ["apple", "pear"] => "" "apple", "pear" ""
    /// </summary>
    /// <param name="items"></param>
    /// <param name="marker"></param>
    /// <param name="separator"></param>
    /// <returns>A string connecting all surrounded items with a given separator </returns>
    public static string ToMarkedString(this IEnumerable<string> items, string marker = "\"", string separator = ",")
    {
        var quotedNames = (
            from item in items
            select $@"{marker}{item}{marker}"
        );
        return string.Join(separator, quotedNames);
    }
}
