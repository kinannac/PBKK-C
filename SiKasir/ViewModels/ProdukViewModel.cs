using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class ProdukViewModel : ViewModelBase
    {
        private readonly ProdukRepository _repo = new();
        private readonly KategoriRepository _katRepo = new();
        private int _loadVersion;

        private string _search = "";
        private ProdukDto? _selected;
        private int _editId;
        private string _nama = "", _harga = "", _stok = "", _newKategori = "";
        private KategoriDto? _kategori;
        private string _message = "", _emptyText = "";
        private bool _isError;

        public ObservableCollection<ProdukDto> Items { get; } = new();
        public ObservableCollection<KategoriDto> KategoriList { get; } = new();

        public string Search
        {
            get => _search;
            set { Set(ref _search, value); _ = LoadAsync(); }
        }

        public ProdukDto? Selected
        {
            get => _selected;
            set { Set(ref _selected, value); if (value != null) FillForm(value); }
        }

        public string Nama { get => _nama; set => Set(ref _nama, value); }
        public string Harga { get => _harga; set => Set(ref _harga, value); }
        public string Stok { get => _stok; set => Set(ref _stok, value); }
        public KategoriDto? SelectedKategori { get => _kategori; set => Set(ref _kategori, value); }
        public string NewKategori { get => _newKategori; set => Set(ref _newKategori, value); }

        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsError { get => _isError; set => Set(ref _isError, value); }
        public string EmptyText { get => _emptyText; set => Set(ref _emptyText, value); }
        public string FormTitle => _editId == 0 ? "Tambah Produk" : "Ubah Produk";

        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddKategoriCommand { get; }

        public ProdukViewModel()
        {
            SaveCommand = new RelayCommand(_ => Save());
            NewCommand = new RelayCommand(_ => { NewForm(); SetInfo(""); });
            DeleteCommand = new RelayCommand(_ => Delete(), _ => Selected != null);
            AddKategoriCommand = new RelayCommand(_ => AddKategori());
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            await LoadKategoriAsync();
            await LoadAsync();
        }

        private async Task LoadKategoriAsync(int selectId = 0)
        {
            try
            {
                var data = await Task.Run(() => _katRepo.GetSemua());
                KategoriList.Clear();
                foreach (var k in data) KategoriList.Add(k);
                if (selectId != 0) SelectedKategori = KategoriList.FirstOrDefault(k => k.IdKategori == selectId);
            }
            catch (Exception) { SetError("Gagal memuat kategori. Periksa koneksi database."); }
        }

        private async Task LoadAsync()
        {
            int version = ++_loadVersion;
            try
            {
                string key = Search.Trim();
                var data = await Task.Run(() => _repo.Cari(key));
                if (version != _loadVersion) return;
                Items.Clear();
                foreach (var d in data) Items.Add(d);
                EmptyText = Items.Count == 0 ? "Produk tidak ditemukan." : "";
            }
            catch (Exception) { SetError("Gagal memuat produk. Periksa koneksi database."); }
        }

        private void FillForm(ProdukDto p)
        {
            _editId = p.IdProduk;
            Nama = p.NamaProduk;
            Harga = p.Harga.ToString("0");
            Stok = p.Stok.ToString();
            SelectedKategori = KategoriList.FirstOrDefault(k => k.IdKategori == p.IdKategori);
            Raise(nameof(FormTitle));
            SetInfo("");
        }

        private void NewForm()
        {
            Selected = null;
            _editId = 0;
            Nama = ""; Harga = ""; Stok = "";
            SelectedKategori = null;
            Raise(nameof(FormTitle));
        }

        private async void Save()
        {
            string nama = Nama.Trim();
            if (nama.Length == 0) { SetError("Nama produk wajib diisi."); return; }
            if (nama.Length > 100) { SetError("Nama produk maksimal 100 karakter."); return; }
            if (SelectedKategori == null) { SetError("Pilih kategori terlebih dahulu."); return; }

            string hargaDigits = new string(Harga.Where(char.IsDigit).ToArray());
            if (!decimal.TryParse(hargaDigits, out var harga) || harga <= 0)
            { SetError("Harga harus berupa angka lebih dari 0."); return; }

            if (!int.TryParse(Stok.Trim(), out var stok) || stok < 0)
            { SetError("Stok harus berupa angka 0 atau lebih."); return; }

            var dto = new ProdukDto
            {
                IdProduk = _editId,
                NamaProduk = nama,
                Harga = harga,
                Stok = stok,
                IdKategori = SelectedKategori.IdKategori
            };

            try
            {
                bool baru = _editId == 0;
                await Task.Run(() => { if (baru) _repo.Tambah(dto); else _repo.Ubah(dto); });
                await LoadAsync();
                NewForm();
                SetInfo(baru ? "Produk berhasil ditambahkan." : "Produk berhasil diperbarui.");
            }
            catch (Exception) { SetError("Gagal menyimpan produk. Periksa koneksi database."); }
        }

        private async void Delete()
        {
            if (Selected == null) return;
            var target = Selected;

            var jawab = MessageBox.Show($"Hapus produk \"{target.NamaProduk}\"?", "Konfirmasi hapus",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (jawab != MessageBoxResult.Yes) return;

            try
            {
                await Task.Run(() => _repo.Hapus(target.IdProduk));
                await LoadAsync();
                NewForm();
                SetInfo("Produk berhasil dihapus.");
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                SetError("Produk tidak bisa dihapus karena sudah punya riwayat transaksi.");
            }
            catch (Exception) { SetError("Gagal menghapus produk. Periksa koneksi database."); }
        }

        private async void AddKategori()
        {
            string nama = NewKategori.Trim();
            if (nama.Length == 0) { SetError("Nama kategori wajib diisi."); return; }
            if (nama.Length > 50) { SetError("Nama kategori maksimal 50 karakter."); return; }
            if (KategoriList.Any(k => k.NamaKategori.Equals(nama, StringComparison.OrdinalIgnoreCase)))
            { SetError("Kategori sudah ada."); return; }

            try
            {
                int id = await Task.Run(() => _katRepo.Tambah(nama));
                await LoadKategoriAsync(id);
                NewKategori = "";
                SetInfo("Kategori berhasil ditambahkan.");
            }
            catch (Exception) { SetError("Gagal menambah kategori. Periksa koneksi database."); }
        }

        private void SetError(string m) { Message = m; IsError = true; }
        private void SetInfo(string m) { Message = m; IsError = false; }
    }
}