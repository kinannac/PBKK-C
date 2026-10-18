using System.Windows;
using System.Windows.Controls;
using SiKasir.ViewModels;
using SiKasir.Views.Auth;

namespace SiKasir.Views.Installer
{
    public partial class InstallerWindow : Window
    {
        private readonly InstallerViewModel _vm = new();

        public InstallerWindow()
        {
            InitializeComponent();
            _vm.Finished += () =>
            {
                new LoginWindow().Show();
                Close();
            };
            Closing += (s, e) => e.Cancel = _vm.IsBusy; // jangan tutup saat instalasi berjalan
            DataContext = _vm;
        }

        // PasswordBox tidak bisa di-binding, jadi nilainya diteruskan ke ViewModel di sini
        private void DbPassword_Changed(object sender, RoutedEventArgs e) => _vm.DbPassword = ((PasswordBox)sender).Password;
        private void AdminPass_Changed(object sender, RoutedEventArgs e) => _vm.AdminPass = ((PasswordBox)sender).Password;
        private void AdminPass2_Changed(object sender, RoutedEventArgs e) => _vm.AdminPass2 = ((PasswordBox)sender).Password;
    }
}