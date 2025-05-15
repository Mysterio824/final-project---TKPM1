using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IPv4RangeExpanderTool
{
    class IPv4RangeExpanderTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Calculate subnet from start and end IP
        public Dictionary<string, string> CalculateSubnet(string startIp, string endIp)
        {
            var result = new Dictionary<string, string>();

            // Validate IPs
            if (!IPAddress.TryParse(startIp, out IPAddress startAddress) ||
                !IPAddress.TryParse(endIp, out IPAddress endAddress))
            {
                return null;
            }

            // Convert to integers for comparison
            uint startInt = BitConverter.ToUInt32(startAddress.GetAddressBytes().Reverse().ToArray(), 0);
            uint endInt = BitConverter.ToUInt32(endAddress.GetAddressBytes().Reverse().ToArray(), 0);

            // Ensure start is less than end
            if (startInt > endInt)
            {
                return null;
            }

            // Add original values to result
            result.Add("StartAddress", startIp);
            result.Add("EndAddress", endIp);

            // Calculate addresses in range
            uint addressCount = endInt - startInt + 1;
            result.Add("AddressCount", addressCount.ToString());

            // Find CIDR
            int cidr = 32;
            uint mask = 0xffffffff;

            while ((startInt & mask) != (endInt & mask) && cidr > 0)
            {
                cidr--;
                mask <<= 1;
            }

            // Calculate network address (the first address in subnet)
            uint networkInt = startInt & mask;
            byte[] networkBytes = BitConverter.GetBytes(networkInt).Reverse().ToArray();
            string networkAddress = new IPAddress(networkBytes).ToString();

            // Calculate broadcast address (the last address in subnet)
            uint broadcastInt = networkInt | ~mask;
            byte[] broadcastBytes = BitConverter.GetBytes(broadcastInt).Reverse().ToArray();
            string broadcastAddress = new IPAddress(broadcastBytes).ToString();

            // Add calculated values
            result.Add("NewStartAddress", networkAddress);
            result.Add("NewEndAddress", broadcastAddress);
            result.Add("NewAddressCount", ((broadcastInt - networkInt) + 1).ToString());
            result.Add("CIDR", $"{networkAddress}/{cidr}");

            return result;
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new IPv4RangeExpanderToolUI(this);
        }
    }
}
