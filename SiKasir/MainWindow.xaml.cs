using System.Windows;
using SiKasir.ViewModels;
using SiKasir.Views.Auth;

namespace SiKasir
{
    public partial class MainWindow : Window
    {
        public MainWindow(string role, int userId)
        {
            InitializeComponent();
            var vm = new MainViewModel(role, userId);
            vm.LogoutRequested += () =>
            {
                new LoginWindow().Show();
                Close();
            };
            DataContext = vm;
        }
    }
}