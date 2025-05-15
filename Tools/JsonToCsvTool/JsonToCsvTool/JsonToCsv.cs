using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace JsonToCsvTool
{
    class JsonToCsvTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to convert JSON to CSV
        public string ConvertJsonToCsv(string jsonInput)
        {
            try
            {
                // Parse JSON using System.Text.Json
                var jsonDoc = JsonDocument.Parse(jsonInput);
                var root = jsonDoc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                    return string.Empty;

                var headers = new HashSet<string>();
                var rows = new List<Dictionary<string, string>>();

                foreach (var element in root.EnumerateArray())
                {
                    var row = new Dictionary<string, string>();

                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in element.EnumerateObject())
                        {
                            headers.Add(property.Name);
                            row[property.Name] = property.Value.ToString();
                        }
                    }

                    rows.Add(row);
                }

                // Build CSV
                var csv = new StringBuilder();

                // Add headers
                var headerList = new List<string>(headers);
                csv.AppendLine(string.Join(",", headerList.ConvertAll(EscapeCsvField)));

                // Add rows
                foreach (var row in rows)
                {
                    var line = new List<string>();
                    foreach (var header in headerList)
                    {
                        row.TryGetValue(header, out string value);
                        line.Add(EscapeCsvField(value ?? string.Empty));
                    }
                    csv.AppendLine(string.Join(",", line));
                }

                return csv.ToString();
            }
            catch
            {
                return null; // Invalid JSON
            }
        }

        private string EscapeCsvField(string field)
        {
            if (field.Contains('"') || field.Contains(',') || field.Contains('\n'))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }
            return field;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new JsonToCsvToolUI(this);
        }
    }
}
