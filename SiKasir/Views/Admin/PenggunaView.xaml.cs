using System.Windows;
using System.Windows.Controls;
using SiKasir.ViewModels;

namespace SiKasir.Views.Admin
{
    public partial class PenggunaView : UserControl
    {
        public PenggunaView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                if (e.NewValue is PenggunaViewModel vm)
                    vm.PasswordsCleared += () => { PwBox.Clear(); Pw2Box.Clear(); };
            };
        }

        private void Pw_Changed(object sender, RoutedEventArgs e)
        {
            if (DataContext is PenggunaViewModel vm) vm.Password = ((PasswordBox)sender).Password;
        }

        private void Pw2_Changed(object sender, RoutedEventArgs e)
        {
            if (DataContext is PenggunaViewModel vm) vm.Password2 = ((PasswordBox)sender).Password;
        }
    }
}