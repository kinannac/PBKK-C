using System.Windows;
using SiKasir.ViewModels;

namespace SiKasir.Views.Auth
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            vm.LoginSucceeded += (role, userId) =>
            {
                var main = new MainWindow(role, userId);
                main.Show();
                Close();
            };
            DataContext = vm;
        }
    }
}