using System;
using System.Threading.Tasks;
using System.Windows.Input;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class DashboardKasirViewModel : ViewModelBase
    {
        private readonly StatistikRepository _repo = new();
        private string _totalTransaksi = "0";
        private string _totalBarang = "0";
        private string _message = "";
        private bool _isLoading;

        public string TotalTransaksi { get => _totalTransaksi; set => Set(ref _totalTransaksi, value); }
        public string TotalBarang { get => _totalBarang; set => Set(ref _totalBarang, value); }
        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }

        public ICommand OpenPosCommand { get; }

        public DashboardKasirViewModel(int userId, Action onOpenPos)
        {
            OpenPosCommand = new RelayCommand(_ => onOpenPos());
            _ = LoadAsync(userId);
        }

        private async Task LoadAsync(int userId)
        {
            IsLoading = true;
            try
            {
                var stat = await Task.Run(() => _repo.GetStatistikKasirHariIni(userId));
                TotalTransaksi = stat.JumlahTransaksi.ToString();
                TotalBarang = stat.TotalBarang.ToString();
            }
            catch (Exception)
            {
                Message = "Gagal memuat data. Periksa koneksi database.";
            }
            finally { IsLoading = false; }
        }
    }
}