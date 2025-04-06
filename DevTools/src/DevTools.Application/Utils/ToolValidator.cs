using System;
using System.Linq;
using System.Reflection;

namespace DevTools.Application.Utils
{
    public static class ToolValidator
    {
        public static bool IsValidTool(string filePath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(filePath);

                // Check all types for the required methods
                return assembly.GetTypes().Any(t =>
                {
                    var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                    var hasExecute = methods.Any(m =>
                        m.Name == "Execute" &&
                        m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType == typeof(object));

                    var hasGetUI = methods.Any(m => m.Name == "GetUI");

                    return hasExecute && hasGetUI;
                });
            }
            catch
            {
                return false;
            }
        }
    }
}
