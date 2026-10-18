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
    public class KeranjangItem : ViewModelBase
    {
        public int IdProduk { get; set; }
        public string Nama { get; set; } = "";
        public decimal HargaSatuan { get; set; }
        public int Stok { get; set; }

        private int _qty = 1;
        public int Qty
        {
            get => _qty;
            set { Set(ref _qty, value); Raise(nameof(Subtotal)); }
        }
        public decimal Subtotal => Qty * HargaSatuan;
    }

    public class PosViewModel : ViewModelBase
    {
        private static readonly CultureInfo Id = new("id-ID");
        private readonly ProdukRepository _produkRepo = new();
        private readonly TransactionRepository _trxRepo = new();
        private readonly int _userId;
        private int _loadVersion;

        private string _search = "";
        private string _uangDibayar = "";
        private decimal _total;
        private string _kembalianText = "-";
        private string _message = "";
        private bool _isError;
        private bool _isBusy;
        private string _emptyText = "";

        public ObservableCollection<ProdukDto> Produk { get; } = new();
        public ObservableCollection<KeranjangItem> Keranjang { get; } = new();

        public string Search
        {
            get => _search;
            set { Set(ref _search, value); _ = LoadProdukAsync(); }
        }
        public string UangDibayar
        {
            get => _uangDibayar;
            set { Set(ref _uangDibayar, value); UpdateKembalian(); }
        }
        public decimal Total { get => _total; set => Set(ref _total, value); }
        public string KembalianText { get => _kembalianText; set => Set(ref _kembalianText, value); }
        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsError { get => _isError; set => Set(ref _isError, value); }
        public bool IsBusy { get => _isBusy; set => Set(ref _isBusy, value); }
        public string EmptyText { get => _emptyText; set => Set(ref _emptyText, value); }

        public ICommand AddCommand { get; }
        public ICommand PlusCommand { get; }
        public ICommand MinusCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand ClearCommand { get; }

        public PosViewModel(int userId)
        {
            _userId = userId;
            AddCommand = new RelayCommand(p => { if (p is ProdukDto d) AddToCart(d); });
            PlusCommand = new RelayCommand(p => { if (p is KeranjangItem k) Plus(k); });
            MinusCommand = new RelayCommand(p => { if (p is KeranjangItem k) Minus(k); });
            RemoveCommand = new RelayCommand(p => { if (p is KeranjangItem k) { Keranjang.Remove(k); Recalc(); } });
            CheckoutCommand = new RelayCommand(_ => Checkout(), _ => !IsBusy);
            ClearCommand = new RelayCommand(_ => { Keranjang.Clear(); UangDibayar = ""; Recalc(); SetInfo(""); });
            _ = LoadProdukAsync();
        }

        private async Task LoadProdukAsync()
        {
            int version = ++_loadVersion;
            try
            {
                string key = Search.Trim();
                var data = await Task.Run(() => _produkRepo.Cari(key));
                if (version != _loadVersion) return;
                Produk.Clear();
                foreach (var d in data) Produk.Add(d);
                EmptyText = Produk.Count == 0 ? "Produk tidak ditemukan." : "";
            }
            catch (Exception)
            {
                SetError("Gagal memuat produk. Periksa koneksi database.");
            }
        }

        private void AddToCart(ProdukDto p)
        {
            if (p.Stok <= 0) { SetError($"Stok {p.NamaProduk} habis."); return; }
            var item = Keranjang.FirstOrDefault(k => k.IdProduk == p.IdProduk);
            if (item == null)
                Keranjang.Add(new KeranjangItem { IdProduk = p.IdProduk, Nama = p.NamaProduk, HargaSatuan = p.Harga, Stok = p.Stok, Qty = 1 });
            else if (item.Qty >= item.Stok) { SetError("Jumlah melebihi stok tersedia."); return; }
            else item.Qty++;
            SetInfo("");
            Recalc();
        }

        private void Plus(KeranjangItem k)
        {
            if (k.Qty >= k.Stok) { SetError("Jumlah melebihi stok tersedia."); return; }
            k.Qty++;
            SetInfo("");
            Recalc();
        }

        private void Minus(KeranjangItem k)
        {
            if (k.Qty > 1) k.Qty--; else Keranjang.Remove(k);
            Recalc();
        }

        private void Recalc()
        {
            Total = Keranjang.Sum(k => k.Subtotal);
            UpdateKembalian();
        }

        private void UpdateKembalian()
        {
            if (!TryParseMoney(UangDibayar, out var bayar)) KembalianText = "-";
            else if (bayar < Total) KembalianText = "Uang kurang";
            else KembalianText = Rp(bayar - Total);
        }

        private async void Checkout()
        {
            if (Keranjang.Count == 0) { SetError("Keranjang masih kosong."); return; }
            if (!TryParseMoney(UangDibayar, out var bayar) || bayar < Total)
            {
                SetError("Uang dibayar kurang dari total.");
                return;
            }

            IsBusy = true;
            try
            {
                var items = Keranjang.Select(k => new ItemKeranjang
                {
                    IdProduk = k.IdProduk,
                    Qty = k.Qty,
                    HargaSatuan = k.HargaSatuan
                }).ToList();

                var hasil = await Task.Run(() => _trxRepo.ProsesCheckout(_userId, bayar, items));
                if (hasil.Success)
                {
                    Keranjang.Clear();
                    UangDibayar = "";
                    Recalc();
                    SetInfo($"Transaksi berhasil. Total {Rp(hasil.Total)}, kembalian {Rp(hasil.Kembalian)}.");
                }
                else SetError(hasil.Message);

                await LoadProdukAsync(); // segarkan stok dan harga di daftar produk
            }
            catch (Exception)
            {
                SetError("Tidak dapat terhubung ke database.");
            }
            finally { IsBusy = false; }
        }

        private static bool TryParseMoney(string s, out decimal value)
        {
            string digits = new string(s.Where(char.IsDigit).ToArray());
            return decimal.TryParse(digits, out value) && digits.Length > 0;
        }

        private static string Rp(decimal v) => "Rp " + v.ToString("N0", Id);
        private void SetError(string m) { Message = m; IsError = true; }
        private void SetInfo(string m) { Message = m; IsError = false; }
    }
}