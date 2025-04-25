using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace JsonToXmlTool
{
    class JsonToXmlTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to convert JSON to XML
        public string ConvertJsonToXml(string jsonContent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonContent))
                    return string.Empty;

                // Parse the JSON content using System.Text.Json
                JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);

                // Create an XML document
                var xmlDoc = new XmlDocument();
                var rootName = "root";
                xmlDoc.AppendChild(xmlDoc.CreateElement(rootName));

                // Process JSON to XML and populate the XML doc
                ProcessJsonToXml(jsonDoc.RootElement, xmlDoc.DocumentElement, xmlDoc);

                // Create a StringWriter to hold the XML
                using (var stringWriter = new System.IO.StringWriter())
                using (var xmlWriter = new XmlTextWriter(stringWriter))
                {
                    xmlWriter.Formatting = Formatting.Indented;

                    // Write the XML document to string
                    xmlDoc.WriteContentTo(xmlWriter);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error converting JSON to XML: {ex.Message}";
            }
        }

        // Recursive method to process JSON elements into XML
        private void ProcessJsonToXml(JsonElement jsonElement, XmlNode parentNode, XmlDocument xmlDoc)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in jsonElement.EnumerateObject())
                {
                    if (property.Name == "_attributes" && property.Value.ValueKind == JsonValueKind.Object)
                    {
                        // Handle attributes in _attributes section
                        foreach (var attr in property.Value.EnumerateObject())
                        {
                            XmlAttribute xmlAttr = xmlDoc.CreateAttribute(attr.Name);
                            xmlAttr.Value = attr.Value.ToString();
                            parentNode.Attributes.Append(xmlAttr);
                        }
                    }
                    else
                    {
                        // Handle properties and nested objects
                        XmlElement element = xmlDoc.CreateElement(property.Name);
                        parentNode.AppendChild(element);
                        ProcessJsonToXml(property.Value, element, xmlDoc); // Recursive call for nested objects
                    }
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                // Handle arrays
                foreach (var item in jsonElement.EnumerateArray())
                {
                    XmlElement element = xmlDoc.CreateElement("item");
                    parentNode.AppendChild(element);
                    ProcessJsonToXml(item, element, xmlDoc); // Recursive call for array items
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String || jsonElement.ValueKind == JsonValueKind.Number || jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
            {
                // Handle primitive values (string, number, or boolean)
                parentNode.InnerText = jsonElement.ToString();
            }
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new JsonToXmlToolUI(this);
        }
    }
}
