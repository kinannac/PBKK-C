using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthRepository _auth = new();
        private string _username = "";
        private string _errorMessage = "";
        private bool _isBusy;

        public string Username { get => _username; set => Set(ref _username, value); }
        public string ErrorMessage { get => _errorMessage; set => Set(ref _errorMessage, value); }
        public bool IsBusy { get => _isBusy; set => Set(ref _isBusy, value); }

        public ICommand LoginCommand { get; }
        public event Action<string, int>? LoginSucceeded;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(DoLogin, _ => !IsBusy);
        }

        private async void DoLogin(object? parameter)
        {
            string pass = (parameter as PasswordBox)?.Password ?? "";
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrEmpty(pass))
            {
                ErrorMessage = "Username dan password wajib diisi.";
                return;
            }

            IsBusy = true;
            ErrorMessage = "";
            try
            {
                string user = Username.Trim();
                var (role, id) = await Task.Run(() =>
                {
                    string r = _auth.Login(user, pass, out int uid);
                    return (r, uid);
                });

                if (string.IsNullOrEmpty(role))
                    ErrorMessage = "Username atau password salah.";
                else
                    LoginSucceeded?.Invoke(role, id);
            }
            catch (SqlException)
            {
                ErrorMessage = "Tidak dapat terhubung ke database. Periksa koneksi server.";
            }
            catch (Exception)
            {
                ErrorMessage = "Terjadi kesalahan. Silakan coba lagi.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}