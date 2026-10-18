using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class PenggunaViewModel : ViewModelBase
    {
        private readonly UserRepository _repo = new();
        private readonly int _currentUserId;

        private UserDto? _selected;
        private string _nama = "", _username = "", _role = "Kasir";
        private string _message = "";
        private bool _isError;

        public ObservableCollection<UserDto> Items { get; } = new();
        public string[] Roles { get; } = { "Kasir", "Admin" };

        // Diisi dari code-behind karena PasswordBox tidak bisa di-binding
        public string Password { get; set; } = "";
        public string Password2 { get; set; } = "";
        public event Action? PasswordsCleared;

        public UserDto? Selected
        {
            get => _selected;
            set { Set(ref _selected, value); Raise(nameof(SelectedInfo)); Raise(nameof(ToggleText)); }
        }
        public string SelectedInfo => Selected == null ? "Belum ada yang dipilih" : $"{Selected.NamaLengkap} ({Selected.Username})";
        public string ToggleText => Selected?.IsActive == false ? "Aktifkan pengguna" : "Nonaktifkan pengguna";

        public string Nama { get => _nama; set => Set(ref _nama, value); }
        public string Username { get => _username; set => Set(ref _username, value); }
        public string Role { get => _role; set => Set(ref _role, value); }

        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsError { get => _isError; set => Set(ref _isError, value); }

        public ICommand AddCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ToggleCommand { get; }

        public PenggunaViewModel(int currentUserId)
        {
            _currentUserId = currentUserId;
            AddCommand = new RelayCommand(_ => Add());
            ResetPasswordCommand = new RelayCommand(_ => ResetPassword(), _ => Selected != null);
            ToggleCommand = new RelayCommand(_ => Toggle(), _ => Selected != null);
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var data = await Task.Run(() => _repo.GetAll());
                Items.Clear();
                foreach (var u in data) Items.Add(u);
            }
            catch (Exception) { SetError("Gagal memuat pengguna. Periksa koneksi database."); }
        }

        private async void Add()
        {
            string nama = Nama.Trim(), user = Username.Trim(), role = Role, pw = Password;

            if (nama.Length == 0) { SetError("Nama lengkap wajib diisi."); return; }
            if (nama.Length > 100) { SetError("Nama lengkap maksimal 100 karakter."); return; }
            if (!Regex.IsMatch(user, "^[A-Za-z0-9_.]{3,50}$"))
            { SetError("Username 3 sampai 50 karakter: huruf, angka, titik, atau garis bawah."); return; }
            if (!ValidatePassword()) return;

            try
            {
                await Task.Run(() => _repo.Tambah(user, nama, role, pw));
                await LoadAsync();
                Nama = ""; Username = ""; Role = "Kasir";
                ClearPasswords();
                SetInfo($"Pengguna {user} berhasil ditambahkan.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                SetError("Username sudah dipakai. Gunakan username lain.");
            }
            catch (Exception) { SetError("Gagal menambah pengguna. Periksa koneksi database."); }
        }

        private async void ResetPassword()
        {
            if (Selected == null) return;
            var target = Selected;
            if (!ValidatePassword()) return;
            string pw = Password;

            try
            {
                await Task.Run(() => _repo.ResetPassword(target.IdUser, pw));
                ClearPasswords();
                SetInfo($"Password {target.Username} berhasil diganti.");
            }
            catch (Exception) { SetError("Gagal mengganti password. Periksa koneksi database."); }
        }

        private async void Toggle()
        {
            if (Selected == null) return;
            var target = Selected;

            if (target.IdUser == _currentUserId && target.IsActive)
            { SetError("Anda tidak bisa menonaktifkan akun yang sedang dipakai."); return; }

            bool aktifBaru = !target.IsActive;
            if (!aktifBaru)
            {
                var jawab = MessageBox.Show($"Nonaktifkan pengguna \"{target.Username}\"? Akun ini tidak akan bisa login.",
                    "Konfirmasi", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (jawab != MessageBoxResult.Yes) return;
            }

            try
            {
                await Task.Run(() => _repo.SetAktif(target.IdUser, aktifBaru));
                await LoadAsync();
                SetInfo(aktifBaru ? $"Pengguna {target.Username} diaktifkan." : $"Pengguna {target.Username} dinonaktifkan.");
            }
            catch (InvalidOperationException ex) { SetError(ex.Message); }
            catch (Exception) { SetError("Gagal mengubah status. Periksa koneksi database."); }
        }

        private bool ValidatePassword()
        {
            if (Password.Length < 6) { SetError("Password minimal 6 karakter."); return false; }
            if (Password != Password2) { SetError("Konfirmasi password tidak sama."); return false; }
            return true;
        }

        private void ClearPasswords()
        {
            Password = ""; Password2 = "";
            PasswordsCleared?.Invoke();
        }

        private void SetError(string m) { Message = m; IsError = true; }
        private void SetInfo(string m) { Message = m; IsError = false; }
    }
}