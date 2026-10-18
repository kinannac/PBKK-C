using System;
using System.Windows.Input;
using SiKasir.Helpers;

namespace SiKasir.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private object _currentView = null!;
        public object CurrentView { get => _currentView; set => Set(ref _currentView, value); }

        public string Role { get; }
        public int UserId { get; }
        public bool IsAdmin { get; }
        public bool IsKasir => !IsAdmin;

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowProdukCommand { get; }
        public event Action? LogoutRequested;

        public MainViewModel(string role, int userId)
        {
            Role = role;
            UserId = userId;
            IsAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                   || role.Equals("Owner", StringComparison.OrdinalIgnoreCase);

            ShowDashboardCommand = new RelayCommand(_ => ShowDashboard());
            ShowPosCommand = new RelayCommand(_ => ShowPos());
            ShowProdukCommand = new RelayCommand(_ => ShowProduk());
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke());

            ShowDashboard();
        }

        private void ShowPos() => CurrentView = new PosViewModel(UserId);

        private void ShowDashboard()
        {
            if (IsAdmin) CurrentView = new DashboardAdminViewModel();
            else CurrentView = new DashboardKasirViewModel(UserId, ShowPos);
        }
        private void ShowProduk()
        {
            if (IsAdmin) CurrentView = new ProdukViewModel();
        }
    }
}