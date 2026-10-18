using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class StokItem
    {
        public string Nama { get; set; } = "";
        public int Stok { get; set; }
    }

    public class BarItem
    {
        public string Label { get; set; } = "";
        public string ValueText { get; set; } = "";
        public double BarHeight { get; set; }
    }

    public class AktivitasItem
    {
        public string Teks { get; set; } = "";
        public string Waktu { get; set; } = "";
    }

    public class DashboardAdminViewModel : ViewModelBase
    {
        private static readonly CultureInfo Id = new("id-ID");
        private const double MaxBar = 150;

        private readonly StatistikRepository _stat = new();
        private readonly DashboardRepository _dash = new();

        private string _totalPendapatan = "Rp 0";
        private string _totalProduk = "0";
        private string _jumlahTransaksi = "0";
        private string _totalPelanggan = "0";
        private string _message = "";
        private string _stokKosongText = "";
        private string _aktivitasKosongText = "";
        private bool _isLoading;

        public string TotalPendapatan { get => _totalPendapatan; set => Set(ref _totalPendapatan, value); }
        public string TotalProduk { get => _totalProduk; set => Set(ref _totalProduk, value); }
        public string JumlahTransaksi { get => _jumlahTransaksi; set => Set(ref _jumlahTransaksi, value); }
        public string TotalPelanggan { get => _totalPelanggan; set => Set(ref _totalPelanggan, value); }
        public string Message { get => _message; set => Set(ref _message, value); }
        public string StokKosongText { get => _stokKosongText; set => Set(ref _stokKosongText, value); }
        public string AktivitasKosongText { get => _aktivitasKosongText; set => Set(ref _aktivitasKosongText, value); }
        public bool IsLoading { get => _isLoading; set => Set(ref _isLoading, value); }

        public ObservableCollection<StokItem> StokMenipis { get; } = new();
        public ObservableCollection<BarItem> Penjualan { get; } = new();
        public ObservableCollection<AktivitasItem> Aktivitas { get; } = new();

        public ICommand RefreshCommand { get; }

        public DashboardAdminViewModel()
        {
            RefreshCommand = new RelayCommand(_ => { _ = LoadAsync(); });
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsLoading = true;
            Message = "";
            try
            {
                var ringkasan = await Task.Run(() => _stat.GetRingkasanAdmin());
                var harian = await Task.Run(() => _stat.GetPenjualan7Hari());
                var stok = await Task.Run(() => _dash.GetProdukStokMenipis());
                var log = await Task.Run(() => _stat.GetAktivitasKasirTerbaru(10));

                TotalPendapatan = "Rp " + ringkasan.PendapatanBulanIni.ToString("N0", Id);
                TotalProduk = ringkasan.TotalProduk.ToString();
                JumlahTransaksi = ringkasan.TransaksiBulanIni.ToString();
                TotalPelanggan = ringkasan.TotalPelanggan.ToString();

                decimal max = harian.Max(h => h.Total);
                Penjualan.Clear();
                foreach (var h in harian)
                {
                    double tinggi = max > 0 ? (double)(h.Total / max) * MaxBar : 0;
                    if (h.Total > 0 && tinggi < 4) tinggi = 4;
                    Penjualan.Add(new BarItem
                    {
                        Label = h.Tanggal.ToString("ddd", Id),
                        ValueText = Singkat(h.Total),
                        BarHeight = tinggi
                    });
                }

                StokMenipis.Clear();
                foreach (System.Data.DataRow r in stok.Rows)
                    StokMenipis.Add(new StokItem
                    {
                        Nama = r["NamaProduk"].ToString() ?? "",
                        Stok = Convert.ToInt32(r["Stok"])
                    });
                StokKosongText = StokMenipis.Count == 0 ? "Tidak ada produk dengan stok menipis." : "";

                Aktivitas.Clear();
                foreach (var a in log)
                    Aktivitas.Add(new AktivitasItem
                    {
                        Teks = $"{a.NamaUser}: {a.Aksi}",
                        Waktu = a.Waktu.ToString("dd MMM HH:mm", Id)
                    });
                AktivitasKosongText = Aktivitas.Count == 0 ? "Belum ada aktivitas kasir." : "";
            }
            catch (Exception)
            {
                Message = "Gagal memuat data. Periksa koneksi database.";
            }
            finally { IsLoading = false; }
        }

        private static string Singkat(decimal v)
        {
            if (v <= 0) return "0";
            if (v >= 1_000_000) return (v / 1_000_000).ToString("0.#", Id) + "jt";
            if (v >= 1_000) return (v / 1_000).ToString("0.#", Id) + "rb";
            return v.ToString("0");
        }
    }
}