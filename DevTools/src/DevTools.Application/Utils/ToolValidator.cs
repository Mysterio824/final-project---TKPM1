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

                var toolType = assembly.GetTypes().FirstOrDefault(t =>
                    t.GetMethod("GetUI") != null &&
                    t.GetMethod("Execute") != null);

                if (toolType == null)
                    return false;

                var getUIMethod = toolType.GetMethod("GetUI");
                var executeMethod = toolType.GetMethod("Execute");

                if (getUIMethod == null ||
                    getUIMethod.ReturnType != typeof(object) ||
                    executeMethod == null ||
                    executeMethod.ReturnType != typeof(object) ||
                    executeMethod.GetParameters().Length != 1 ||
                    executeMethod.GetParameters()[0].ParameterType != typeof(object))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading assembly or finding types: {ex.Message}");
                return false;
            }
        }
    }
}
