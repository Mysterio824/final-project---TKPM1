using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MacAddressLookupTool
{
    class MacAddressLookupTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Dictionary<string, string> macDatabase;
        public MacAddressLookupTool()
        {
            InitializeMacDatabase();
        }
        private void InitializeMacDatabase()
        {
            macDatabase = new Dictionary<string, string>
            {
                { "00:00:0C", "Cisco Systems, Inc\n80 West Tasman Drive\nSan Jose CA 94568\nUnited States" },
                { "00:1A:A0", "Dell Inc.\nOne Dell Way\nRound Rock TX 78682\nUnited States" },
                { "00:25:90", "Apple, Inc.\n1 Infinite Loop\nCupertino CA 95014\nUnited States" },
                { "20:37:06", "Cisco Systems, Inc\n80 West Tasman Drive\nSan Jose CA 94568\nUnited States" },
                { "3C:5A:B4", "Google, Inc\n1600 Amphitheatre Parkway\nMountain View CA 94043\nUnited States" },
                { "B8:27:EB", "Raspberry Pi Foundation\nMaurice Wilkes Building\nCowley Road\nCambridge CB4 0DS\nUnited Kingdom" }
                // Add more entries as needed
            };
        }
        public bool IsValidMacAddress(string macAddress)
        {
            // Regex to validate MAC address format (supports formats like XX:XX:XX:XX:XX:XX, XX-XX-XX-XX-XX-XX)
            string pattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";
            return Regex.IsMatch(macAddress, pattern);
        }
        public string LookupVendor(string macAddress)
        {
            if (!IsValidMacAddress(macAddress))
                return null;
            // Normalize MAC address to XX:XX:XX format for OUI lookup
            string normalized = macAddress.ToUpper().Replace("-", ":");
            string oui = normalized.Substring(0, 8); // Get first 3 bytes (XX:XX:XX)
            if (macDatabase.TryGetValue(oui, out string vendorInfo))
            {
                return vendorInfo;
            }
            return "Unknown vendor for this address";
        }
        public object Execute(object input)
        {
            return input;
        }
        public UserControl GetUI()
        {
            return new MacAddressLookupToolUI(this);
        }
    }
}
