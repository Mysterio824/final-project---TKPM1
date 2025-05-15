using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using DevTools.UI.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DevTools.UI.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly INavigationService _navigationService;

        private string _username;
        private string _email;
        private string _password;
        private string _errorMessage;
        private bool _isErrorMessageVisible;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                (RegisterCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                (RegisterCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                (RegisterCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }
        public bool IsErrorMessageVisible
        {
            get => _isErrorMessageVisible;
            private set
            {
                _isErrorMessageVisible = value;
                OnPropertyChanged();
            }
        }


        public bool IsLoading
        {
            get => _isLoading;
            set 
            {
                SetProperty(ref _isLoading, value);
                (RegisterCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand RegisterCommand { get; }

        public RegisterViewModel(AuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            RegisterCommand = new AsyncCommand(ExecuteRegisterAsync, CanExecuteRegister);
        }

        private bool CanExecuteRegister() => !string.IsNullOrWhiteSpace(Username) &&
                                             !string.IsNullOrWhiteSpace(Email) &&
                                             !string.IsNullOrWhiteSpace(Password) &&
                                             !IsLoading;
        private async Task ExecuteRegisterAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _authService.RegisterAsync(Username, Email, Password);
                if (user != null)
                {
                    ErrorMessage = "To officially become a user, check your email and click the link from DevTools Support to verify email. Navigating to Login Page";
                    IsErrorMessageVisible = true;
                    await Task.Delay(5000);
                    _navigationService.NavigateTo(typeof(LoginPage));
                }
                else
                {
                    ErrorMessage = "Registration failed. Email might be already in use.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
