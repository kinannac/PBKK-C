using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;
using SiKasir.Repositories;

namespace SiKasir.ViewModels
{
    public class InstallerViewModel : ViewModelBase
    {
        private static readonly string[] Titles =
            { "Selamat Datang", "Jaringan dan Port", "Koneksi Database", "Akun Administrator", "Profil Toko", "Selesai" };

        private readonly InstallerRepository _repo = new();

        private int _step;
        private bool _isBusy, _isError, _testOk, _windowsAuth = true;
        private string _message = "", _summary = "";
        private string _server = @".\SQLEXPRESS", _port = "", _dbUser = "sa", _dbPassword = "", _database = "SiKasirDB";
        private string _adminNama = "", _adminUser = "", _adminPass = "", _adminPass2 = "";
        private string _namaToko = "", _alamat = "", _telepon = "";

        public int Step
        {
            get => _step;
            private set
            {
                Set(ref _step, value);
                Raise(nameof(StepInfo)); Raise(nameof(NextText));
                Raise(nameof(ShowNav)); Raise(nameof(ShowBack)); Raise(nameof(ShowFinish));
            }
        }

        public string StepInfo => $"Langkah {Step + 1} dari 6: {Titles[Step]}";
        public string NextText => Step == 4 ? "Mulai instalasi" : "Lanjut";
        public bool ShowNav => Step < 5;
        public bool ShowBack => Step > 0 && Step < 5;
        public bool ShowFinish => Step == 5;

        // Koneksi: mengubah salah satu nilai membatalkan hasil tes sebelumnya
        public string Server { get => _server; set { Set(ref _server, value); ResetTest(); } }
        public string Port { get => _port; set { Set(ref _port, value); ResetTest(); } }
        public string DbUser { get => _dbUser; set { Set(ref _dbUser, value); ResetTest(); } }
        public string DbPassword { get => _dbPassword; set { Set(ref _dbPassword, value); ResetTest(); } }
        public string Database { get => _database; set { Set(ref _database, value); ResetTest(); } }
        public bool WindowsAuth
        {
            get => _windowsAuth;
            set { Set(ref _windowsAuth, value); Raise(nameof(UseSqlAuth)); ResetTest(); }
        }
        public bool UseSqlAuth => !WindowsAuth;

        public string AdminNama { get => _adminNama; set => Set(ref _adminNama, value); }
        public string AdminUser { get => _adminUser; set => Set(ref _adminUser, value); }
        public string AdminPass { get => _adminPass; set => Set(ref _adminPass, value); }
        public string AdminPass2 { get => _adminPass2; set => Set(ref _adminPass2, value); }

        public string NamaToko { get => _namaToko; set => Set(ref _namaToko, value); }
        public string Alamat { get => _alamat; set => Set(ref _alamat, value); }
        public string Telepon { get => _telepon; set => Set(ref _telepon, value); }

        public string Message { get => _message; set => Set(ref _message, value); }
        public bool IsError { get => _isError; set => Set(ref _isError, value); }
        public string Summary { get => _summary; set => Set(ref _summary, value); }
        public bool TestOk { get => _testOk; private set => Set(ref _testOk, value); }
        public bool IsBusy
        {
            get => _isBusy;
            set { Set(ref _isBusy, value); CommandManager.InvalidateRequerySuggested(); }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand TestCommand { get; }
        public ICommand FinishCommand { get; }
        public event Action? Finished;

        public InstallerViewModel()
        {
            NextCommand = new RelayCommand(async _ => await NextAsync(), _ => !IsBusy);
            BackCommand = new RelayCommand(_ => Back(), _ => !IsBusy);
            TestCommand = new RelayCommand(async _ => await TestAsync(), _ => !IsBusy);
            FinishCommand = new RelayCommand(_ => Finished?.Invoke());
        }

        private void ResetTest()
        {
            if (!TestOk) return;
            TestOk = false;
            SetInfo("");
        }

        private void Back()
        {
            SetInfo("");
            if (Step > 0) Step--;
        }

        private async Task NextAsync()
        {
            SetInfo("");
            switch (Step)
            {
                case 0: Step = 1; break;
                case 1: if (ValidateNetwork()) Step = 2; break;
                case 2:
                    if (!TestOk) SetError("Tes koneksi terlebih dahulu sebelum melanjutkan.");
                    else Step = 3;
                    break;
                case 3: if (ValidateAdmin()) Step = 4; break;
                case 4: await InstallAsync(); break;
            }
        }

        private async Task TestAsync()
        {
            if (!ValidateNetwork() || !ValidateDatabaseName()) return;

            IsBusy = true;
            SetInfo("Menguji koneksi...");
            try
            {
                var s = Settings();
                var (ok, msg) = await Task.Run(() => _repo.TestConnection(s));
                TestOk = ok;
                if (ok) SetInfo(msg); else SetError(msg);
            }
            finally { IsBusy = false; }
        }

        private async Task InstallAsync()
        {
            if (!ValidateToko()) return;

            IsBusy = true;
            SetInfo("Memulai instalasi...");
            try
            {
                var s = Settings();
                var data = new InstallData
                {
                    NamaToko = NamaToko.Trim(),
                    Alamat = Alamat.Trim(),
                    Telepon = Telepon.Trim(),
                    AdminNama = AdminNama.Trim(),
                    AdminUser = AdminUser.Trim(),
                    AdminPassword = AdminPass
                };
                var progress = new Progress<string>(m => SetInfo(m));

                await Task.Run(() => _repo.Install(s, data, progress));
                DatabaseConnection.SaveConnectionString(s.BuildConnectionString());

                Summary = $"Server: {s.DataSource}\nDatabase: {s.Database}\nAdministrator: {data.AdminUser}\nToko: {data.NamaToko}";
                SetInfo("");
                Step = 5;
            }
            catch (InstallException ex) { SetError(ex.Message); }
            catch (SqlException ex) { SetError(InstallerRepository.Friendly(ex)); }
            catch (Exception) { SetError("Instalasi gagal. Periksa pengaturan lalu coba lagi."); }
            finally { IsBusy = false; }
        }

        private DbSettings Settings() => new()
        {
            Server = Server.Trim(),
            Port = Port.Trim(),
            WindowsAuth = WindowsAuth,
            User = DbUser.Trim(),
            Password = DbPassword,
            Database = Database.Trim()
        };

        private bool ValidateNetwork()
        {
            if (string.IsNullOrWhiteSpace(Server)) return Fail("Alamat server wajib diisi.");
            if (!string.IsNullOrWhiteSpace(Port) &&
                (!int.TryParse(Port.Trim(), out int p) || p < 1 || p > 65535))
                return Fail("Port harus berupa angka 1 sampai 65535.");
            if (!WindowsAuth && string.IsNullOrWhiteSpace(DbUser))
                return Fail("Username SQL Server wajib diisi.");
            return true;
        }

        private bool ValidateDatabaseName()
        {
            if (!Regex.IsMatch(Database.Trim(), "^[A-Za-z][A-Za-z0-9_]{0,63}$"))
                return Fail("Nama database hanya boleh huruf, angka, dan garis bawah, diawali huruf (maks. 64 karakter).");
            return true;
        }

        private bool ValidateAdmin()
        {
            if (string.IsNullOrWhiteSpace(AdminNama)) return Fail("Nama lengkap wajib diisi.");
            if (AdminNama.Trim().Length > 100) return Fail("Nama lengkap maksimal 100 karakter.");
            if (!Regex.IsMatch(AdminUser.Trim(), "^[A-Za-z0-9_.]{3,50}$"))
                return Fail("Username 3 sampai 50 karakter: huruf, angka, titik, atau garis bawah.");
            if (AdminPass.Length < 6) return Fail("Password minimal 6 karakter.");
            if (AdminPass != AdminPass2) return Fail("Konfirmasi password tidak sama.");
            return true;
        }

        private bool ValidateToko()
        {
            if (string.IsNullOrWhiteSpace(NamaToko)) return Fail("Nama toko wajib diisi.");
            if (NamaToko.Trim().Length > 100) return Fail("Nama toko maksimal 100 karakter.");
            if (Alamat.Trim().Length > 255) return Fail("Alamat maksimal 255 karakter.");
            if (Telepon.Trim().Length > 20 || !Regex.IsMatch(Telepon.Trim(), @"^[0-9+\-\s()]*$"))
                return Fail("Telepon hanya angka (maks. 20 karakter).");
            return true;
        }

        private bool Fail(string m) { SetError(m); return false; }
        private void SetError(string m) { Message = m; IsError = true; }
        private void SetInfo(string m) { Message = m; IsError = false; }
    }
}