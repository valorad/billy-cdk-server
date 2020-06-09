using System.Collections.Generic;
using GraphQL.Language.AST;
using GraphQL.Types;

namespace App.Lib
{
  class JsonGraphTypeConverter : IAstFromValueConverter
  {
    public bool Matches(object value, IGraphType type)
    {
      return type.Name == "Json";
    }

    public IValue Convert(object value, IGraphType type)
    {
      return new JsonGraphValue(value as Dictionary<string, object>);
    }
    
  }


  class JsonGraphValue : ValueNode<Dictionary<string, object>>
  {
    public JsonGraphValue(Dictionary<string, object> value)
    {
      Value = value;
    }

    protected override bool Equals(ValueNode<Dictionary<string, object>> node)
    {
      return Value.Equals(node.Value);
    }
  }

  internal class JsonGraphType : ScalarGraphType
  {
    public JsonGraphType()
    {
      Name = "Json";
    }

    public override object Serialize(object value)
    {
      return ParseValue(value);
    }

    public override object ParseValue(object value)
    {
      if (value == null)
      {
        return null;
      }

      return value;
    }

    public override object ParseLiteral(IValue value)
    {
      // if (value is JsonGraphValue jsonGraphValue)
      // {
      //   return jsonGraphValue.Value;
      // }

      var jsonValue = value as JsonGraphValue;
      return jsonValue?.Value;

      // else
      // {
      //   var tv = value as JsonGraphTypeConverter;
        
      //   return tv?.GetValue();
      // }
    }
  }
}