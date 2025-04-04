using System.Reflection;
using DevTools.Application.Common;

namespace DevTools.Application.Utils
{
    public static class ToolValidator
    {
        public static bool IsValidTool(string dllPath, out Type? toolType)
        {
            toolType = null;

            if (!File.Exists(dllPath) || !Path.GetExtension(dllPath).Equals(".dll", StringComparison.CurrentCultureIgnoreCase))
                return false;

            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                toolType = assembly.GetTypes()
                    .FirstOrDefault(t =>
                        typeof(ITool).IsAssignableFrom(t) &&
                        !t.IsInterface &&
                        !t.IsAbstract);
            }
            catch (ReflectionTypeLoadException)
            {
                return false;
            }
            catch (BadImageFormatException)
            {
                return false;
            }

            return toolType != null;
        }
    }
}