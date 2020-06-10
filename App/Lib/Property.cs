using System;
using System.Linq;
using System.Reflection;

namespace App.Lib
{
  public static class Property
  {
	 public static object GetValue(object source, string propertyName)
	 {
    	var property = source.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
    	return property?.GetValue(source);
	 }
   
  }
}