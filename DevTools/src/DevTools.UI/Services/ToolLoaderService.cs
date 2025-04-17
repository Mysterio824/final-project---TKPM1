using DevTools.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.Services
{
    public class ToolLoaderService
    {
        public async Task<ITool?> LoadAsync(byte[] dllBytes)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".dll");
            await File.WriteAllBytesAsync(tempFilePath, dllBytes);

            var assembly = Assembly.LoadFrom(tempFilePath);

            var toolType = assembly.GetTypes()
                .FirstOrDefault(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (toolType == null) return null;

            return Activator.CreateInstance(toolType) as ITool;
        }
    }
}
