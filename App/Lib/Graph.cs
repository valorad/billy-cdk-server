using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace App.Lib
{
  public static class Graph
  {
    public static string LoadDefinitions()
    {
      string filePath = Path.GetFullPath(@"Models\Graphs\");
      var extensions = new List<string>() { ".graphql", ".gql" };

      List<string> files = Directory.GetFiles(filePath)
        .Where(file => extensions.Any(file.ToLower().EndsWith)).ToList();

      string typeDefs = "";
      // typeDefs += "scalar Json \n";
      // read text from each file and combine
      foreach (var file in files)
      {
        typeDefs += File.ReadAllText(file);
      }

      Console.WriteLine(typeDefs);

      return typeDefs;
    }
  }
}