using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SubnetCalculatorTool
{
    class SubnetCalculatorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new SubnetCalculatorToolUI(this);
        }

        public SubnetInfo CalculateSubnet(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new SubnetInfo { IsValid = false, ErrorMessage = "IP address cannot be empty" };

            // Try to parse CIDR notation (e.g., 192.168.0.1/24)
            string ipPart;
            int cidrPart = 24; // Default to /24 if no mask is provided
            bool hasCidr = false;

            if (input.Contains("/"))
            {
                var parts = input.Split('/');
                if (parts.Length != 2)
                    return new SubnetInfo { IsValid = false, ErrorMessage = "Invalid CIDR format" };

                ipPart = parts[0];
                hasCidr = int.TryParse(parts[1], out cidrPart);

                if (!hasCidr || cidrPart < 0 || cidrPart > 32)
                    return new SubnetInfo { IsValid = false, ErrorMessage = "CIDR must be between 0 and 32" };
            }
            else
            {
                ipPart = input;
            }

            // Try to parse the IP address
            if (!IPAddress.TryParse(ipPart, out IPAddress ipAddress))
                return new SubnetInfo { IsValid = false, ErrorMessage = "Invalid IP address format" };

            // Must be IPv4
            if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                return new SubnetInfo { IsValid = false, ErrorMessage = "Only IPv4 addresses are supported" };

            return CalculateSubnetInfo(ipAddress, cidrPart);
        }

        public SubnetInfo GetPreviousBlock(SubnetInfo currentInfo)
        {
            if (!currentInfo.IsValid)
                return currentInfo;

            try
            {
                // Convert network address to uint, subtract network size, and convert back to IP
                uint networkAddressInt = ConvertIPToUInt(IPAddress.Parse(currentInfo.NetworkAddress));
                uint newNetworkAddressInt = networkAddressInt - currentInfo.NetworkSize;

                // Create IP from uint and calculate subnet
                IPAddress newNetworkIP = ConvertUIntToIP(newNetworkAddressInt);
                return CalculateSubnetInfo(newNetworkIP, currentInfo.Cidr);
            }
            catch
            {
                return new SubnetInfo { IsValid = false, ErrorMessage = "Cannot calculate previous block" };
            }
        }

        public SubnetInfo GetNextBlock(SubnetInfo currentInfo)
        {
            if (!currentInfo.IsValid)
                return currentInfo;

            try
            {
                // Convert network address to uint, add network size, and convert back to IP
                uint networkAddressInt = ConvertIPToUInt(IPAddress.Parse(currentInfo.NetworkAddress));
                uint newNetworkAddressInt = networkAddressInt + currentInfo.NetworkSize;

                // Create IP from uint and calculate subnet
                IPAddress newNetworkIP = ConvertUIntToIP(newNetworkAddressInt);
                return CalculateSubnetInfo(newNetworkIP, currentInfo.Cidr);
            }
            catch
            {
                return new SubnetInfo { IsValid = false, ErrorMessage = "Cannot calculate next block" };
            }
        }

        private SubnetInfo CalculateSubnetInfo(IPAddress ipAddress, int cidr)
        {
            try
            {
                // Create subnet mask from CIDR
                uint mask = ~(uint.MaxValue >> cidr);
                IPAddress subnetMask = ConvertUIntToIP(mask);

                // Calculate network address
                byte[] ipBytes = ipAddress.GetAddressBytes();
                byte[] maskBytes = subnetMask.GetAddressBytes();
                byte[] networkBytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    networkBytes[i] = (byte)(ipBytes[i] & maskBytes[i]);
                }

                IPAddress networkAddress = new IPAddress(networkBytes);

                // Calculate wildcard mask
                byte[] wildcardBytes = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    wildcardBytes[i] = (byte)(~maskBytes[i] & 255);
                }
                IPAddress wildcardMask = new IPAddress(wildcardBytes);

                // Calculate network size
                uint networkSize = (uint)Math.Pow(2, 32 - cidr);

                // Calculate first usable address (network address + 1)
                uint networkInt = ConvertIPToUInt(networkAddress);
                IPAddress firstAddress = ConvertUIntToIP(networkInt + 1);

                // Calculate last usable address (broadcast - 1)
                IPAddress lastAddress = ConvertUIntToIP(networkInt + networkSize - 2);

                // Calculate broadcast address (network + network size - 1)
                IPAddress broadcastAddress = ConvertUIntToIP(networkInt + networkSize - 1);

                // Determine IP class
                string ipClass = GetIpClass(ipAddress);

                // Create subnet info
                var result = new SubnetInfo
                {
                    IsValid = true,
                    InputAddress = ipAddress.ToString(),
                    Cidr = cidr,
                    NetworkAddress = networkAddress.ToString(),
                    NetworkMask = subnetMask.ToString(),
                    WildcardMask = wildcardMask.ToString(),
                    NetworkSize = networkSize,
                    FirstAddress = firstAddress.ToString(),
                    LastAddress = lastAddress.ToString(),
                    BroadcastAddress = broadcastAddress.ToString(),
                    IpClass = ipClass,
                    NetworkMaskBinary = ConvertIPToBinary(subnetMask),
                    CidrNotation = $"/{cidr}"
                };

                return result;
            }
            catch (Exception ex)
            {
                return new SubnetInfo { IsValid = false, ErrorMessage = $"Error calculating subnet information: {ex.Message}" };
            }
        }

        private string GetIpClass(IPAddress ip)
        {
            byte firstOctet = ip.GetAddressBytes()[0];

            if (firstOctet < 128)
                return "A";
            else if (firstOctet < 192)
                return "B";
            else if (firstOctet < 224)
                return "C";
            else if (firstOctet < 240)
                return "D";
            else
                return "E";
        }

        private uint ConvertIPToUInt(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }

        private IPAddress ConvertUIntToIP(uint ipInt)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(ipInt >> 24);
            bytes[1] = (byte)(ipInt >> 16);
            bytes[2] = (byte)(ipInt >> 8);
            bytes[3] = (byte)(ipInt);
            return new IPAddress(bytes);
        }

        private string ConvertIPToBinary(IPAddress ip)
        {
            byte[] bytes = ip.GetAddressBytes();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(Convert.ToString(bytes[i], 2).PadLeft(8, '0'));
                if (i < bytes.Length - 1)
                    sb.Append('.');
            }

            return sb.ToString();
        }
    }

    public class SubnetInfo
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string InputAddress { get; set; }
        public string NetworkAddress { get; set; }
        public string NetworkMask { get; set; }
        public string NetworkMaskBinary { get; set; }
        public string CidrNotation { get; set; }
        public int Cidr { get; set; }
        public string WildcardMask { get; set; }
        public uint NetworkSize { get; set; }
        public string FirstAddress { get; set; }
        public string LastAddress { get; set; }
        public string BroadcastAddress { get; set; }
        public string IpClass { get; set; }
    }
}
