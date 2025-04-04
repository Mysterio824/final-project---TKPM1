using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IToolsApp.Core.Interfaces;
using Microsoft.UI.Xaml.Controls;

namespace HashGeneratorTool
{
    class HashGeneratorTool : ITool
    {
        private int _id;
        private string _name = "Hash Generator Tool";
        private string _description = "Generates hashes using MD5, SHA-1, or SHA-256 algorithms.";
        private bool _isPremium;
        private bool _isFavorite;
        private string _category = "Utility";
        private bool _isAvailable = true;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsPremium
        {
            get => _isPremium;
            set => SetProperty(ref _isPremium, value);
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            set => SetProperty(ref _isAvailable, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Method to generate hash
        public string GenerateHash(string input, string algorithmType)
        {
            switch (algorithmType)
            {
                case "MD5":
                    return GenerateMD5Hash(input);
                case "SHA1":
                    return GenerateSHA1Hash(input);
                case "SHA256":
                    return GenerateSHA256Hash(input);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithmType), algorithmType, null);
            }
        }

        // MD5 Hash
        private string GenerateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-1 Hash
        private string GenerateSHA1Hash(string input)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // SHA-256 Hash
        private string GenerateSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
        public object Execute(object input) { return input; }

        public UserControl GetUI()
        {
            return new HashGeneratorToolUI(this);
        }
    }
}