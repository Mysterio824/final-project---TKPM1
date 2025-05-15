using DevTools.UI.Models;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UuidGeneratorTool
{
    class UuidGeneratorTool : ITool
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<string> GenerateUuids(string version, int quantity, string? namespaceName = null, string? name = null)
        {
            var results = new List<string>();

            for (int i = 0; i < quantity; i++)
            {
                switch (version)
                {
                    case "NIL":
                        results.Add(Guid.Empty.ToString());
                        break;
                    case "v1":
                        results.Add(GenerateV1Uuid());
                        break;
                    case "v3":
                        if (string.IsNullOrEmpty(namespaceName) || string.IsNullOrEmpty(name))
                            throw new ArgumentException("Namespace and name are required for v3 UUIDs");
                        results.Add(GenerateV3Uuid(namespaceName, name));
                        break;
                    case "v4":
                        results.Add(GenerateV4Uuid());
                        break;
                    case "v5":
                        if (string.IsNullOrEmpty(namespaceName) || string.IsNullOrEmpty(name))
                            throw new ArgumentException("Namespace and name are required for v5 UUIDs");
                        results.Add(GenerateV5Uuid(namespaceName, name));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(version), version, null);
                }
            }

            return results;
        }

        private string GenerateV1Uuid()
        {
            byte[] guidArray = Guid.NewGuid().ToByteArray();

            guidArray[7] = (byte)((guidArray[7] & 0x0F) | 0x10);

            guidArray[8] = (byte)((guidArray[8] & 0x3F) | 0x80);

            return new Guid(guidArray).ToString();
        }

        private string GenerateV3Uuid(string namespaceName, string name)
        {
            Guid namespaceGuid;
            switch (namespaceName.ToLower())
            {
                case "dns":
                    namespaceGuid = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "url":
                    namespaceGuid = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "oid":
                    namespaceGuid = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "x500":
                    namespaceGuid = new Guid("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
                    break;
                default:
                    if (!Guid.TryParse(namespaceName, out namespaceGuid))
                    {
                        using (var md5 = MD5.Create())
                        {
                            byte[] hashedBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(namespaceName));
                            namespaceGuid = new Guid(hashedBytes);
                        }
                    }
                    break;
            }

            byte[] namespaceBytes = namespaceGuid.ToByteArray();
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] combined = new byte[namespaceBytes.Length + nameBytes.Length];

            Buffer.BlockCopy(namespaceBytes, 0, combined, 0, namespaceBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, combined, namespaceBytes.Length, nameBytes.Length);

            byte[] hashBytes;
            using (var md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(combined);
            }

            hashBytes[6] = (byte)((hashBytes[6] & 0x0F) | 0x30);

            hashBytes[8] = (byte)((hashBytes[8] & 0x3F) | 0x80);

            return new Guid(hashBytes).ToString();
        }
        private string GenerateV4Uuid()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateV5Uuid(string namespaceName, string name)
        {
            Guid namespaceGuid;
            switch (namespaceName.ToLower())
            {
                case "dns":
                    namespaceGuid = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "url":
                    namespaceGuid = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "oid":
                    namespaceGuid = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");
                    break;
                case "x500":
                    namespaceGuid = new Guid("6ba7b814-9dad-11d1-80b4-00c04fd430c8");
                    break;
                default:
                    if (!Guid.TryParse(namespaceName, out namespaceGuid))
                    {
                        using (var sha1 = SHA1.Create())
                        {
                            byte[] hashedBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(namespaceName));
                            byte[] namespaceByte = new byte[16];
                            Array.Copy(hashedBytes, namespaceByte, 16);
                            namespaceGuid = new Guid(namespaceByte);
                        }
                    }
                    break;
            }

            byte[] namespaceBytes = namespaceGuid.ToByteArray();
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] combined = new byte[namespaceBytes.Length + nameBytes.Length];

            Buffer.BlockCopy(namespaceBytes, 0, combined, 0, namespaceBytes.Length);
            Buffer.BlockCopy(nameBytes, 0, combined, namespaceBytes.Length, nameBytes.Length);

            byte[] hashBytes;
            using (var sha1 = SHA1.Create())
            {
                hashBytes = sha1.ComputeHash(combined);
            }

            byte[] resultBytes = new byte[16];
            Array.Copy(hashBytes, resultBytes, 16);

            resultBytes[6] = (byte)((resultBytes[6] & 0x0F) | 0x50);

            resultBytes[8] = (byte)((resultBytes[8] & 0x3F) | 0x80);

            return new Guid(resultBytes).ToString();
        }

        public object Execute(object input)
        {
            return input;
        }

        public UserControl GetUI()
        {
            return new UuidGeneratorToolUI(this);
        }
    }
}
