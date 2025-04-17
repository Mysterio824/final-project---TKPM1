using DevTools.UI.Services;
using DevTools.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTools.UI.ViewModels
{
    public class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private string _username;
        private string _password;

        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                LoginCommand.RaiseCanExecuteChanged(); // Re-check CanExecute
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                LoginCommand.RaiseCanExecuteChanged(); // Re-check CanExecute
            }
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        public async Task<bool> LoginAsync()
        {
            return await _authService.LoginAsync(Username, Password);
        }
    }
}
