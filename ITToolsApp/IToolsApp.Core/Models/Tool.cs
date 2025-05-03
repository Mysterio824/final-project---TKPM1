using IToolsApp.Core.Interfaces;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IToolsApp.Core.Models
{
    public class Tool
    {
        private int _id;
        private string _name;
        private string _description;
        private bool _isPremium;
        private bool _isFavorite;
        private string _category;
        private bool _isAvailable;
        private UserControl _ui;
        private object _input;
        public Tool()
        {
            // Default values
            IsAvailable = true;
            IsPremium = false;
            IsFavorite = false;
            Category = string.Empty;
        }

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

        // Helper method to raise PropertyChanged event
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper method to set property value and raise PropertyChanged event
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Override ToString() for better debugging/logging
        public override string ToString()
        {
            return $"{Name} (Category: {Category}, Available: {IsAvailable}, Premium: {IsPremium})";
        }
    }
}