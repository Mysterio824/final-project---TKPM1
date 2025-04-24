using DevTools.UI.Models;
using DevTools.UI.Services;
using DevTools.UI.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DevTools.UI.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly Action<User> _onLoginSuccess;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;
        private bool _rememberMe;
        private bool _isPasswordVisible;

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                (LoginCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                (LoginCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                (LoginCommand as AsyncCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        public LoginViewModel(AuthService authService, Action<User> onLoginSuccess)
        {
            _authService = authService;
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new AsyncCommand(ExecuteLoginAsync, CanExecuteLogin);
            TogglePasswordVisibilityCommand = new RelayCommand(ExecuteTogglePasswordVisibility);
        }

        private bool CanExecuteLogin()
        {
            bool canExecute = !string.IsNullOrWhiteSpace(Email) &&
                              !string.IsNullOrWhiteSpace(Password) &&
                              !IsLoading;
            return canExecute;
        }

        private async Task ExecuteLoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                await Task.Delay(500);

                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    if (RememberMe)
                    {
                        var vault = new Windows.Security.Credentials.PasswordVault();
                        var credential = new Windows.Security.Credentials.PasswordCredential(
                            "DevTools",
                            Email,
                            Password
                        );
                        vault.Add(credential);
                    }
                    _onLoginSuccess(user);
                }
                else
                {
                    ErrorMessage = "Invalid email or password. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteTogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }
    }
}