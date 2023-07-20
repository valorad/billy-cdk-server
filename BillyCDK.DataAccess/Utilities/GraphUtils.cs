namespace BillyCDK.DataAccess.Utilities;

public class GraphUtils
{
    public static string LoadDefinitions()
    {
        string filePath = Path.GetFullPath(Path.Combine(".", "Models", "Graphs"));
        var extensions = new List<string>() { ".graphql", ".gql" };

        List<string> files = Directory.GetFiles(filePath)
          .Where(file => extensions.Any(file.ToLower().EndsWith)).ToList();

        string typeDefs = "";

        foreach (var file in files)
        {
            typeDefs += File.ReadAllText(file);
        }

        return typeDefs;
    }
}
