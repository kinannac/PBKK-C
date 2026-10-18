using System;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;

namespace SiKasir.Repositories
{
    public class InstallException : Exception
    {
        public InstallException(string message) : base(message) { }
    }

    public class DbSettings
    {
        public string Server { get; set; } = "";
        public string Port { get; set; } = "";
        public bool WindowsAuth { get; set; } = true;
        public string User { get; set; } = "";
        public string Password { get; set; } = "";
        public string Database { get; set; } = "";

        // Port diabaikan untuk server bernama instance (mis. .\SQLEXPRESS)
        public string DataSource =>
            (Server.Contains('\\') || string.IsNullOrWhiteSpace(Port)) ? Server : $"{Server},{Port}";

        public string BuildConnectionString(string? database = null, int timeout = 15)
        {
            var b = new SqlConnectionStringBuilder
            {
                DataSource = DataSource,
                InitialCatalog = database ?? Database,
                TrustServerCertificate = true,
                ConnectTimeout = timeout
            };
            if (WindowsAuth) b.IntegratedSecurity = true;
            else { b.UserID = User; b.Password = Password; }
            return b.ToString();
        }
    }

    public class InstallData
    {
        public string NamaToko { get; set; } = "";
        public string Alamat { get; set; } = "";
        public string Telepon { get; set; } = "";
        public string AdminNama { get; set; } = "";
        public string AdminUser { get; set; } = "";
        public string AdminPassword { get; set; } = "";
    }

    public class InstallerRepository
    {
        private static readonly string[] SchemaStatements =
        {
            @"IF OBJECT_ID('dbo.ProfilToko','U') IS NULL CREATE TABLE dbo.ProfilToko (
                Id INT PRIMARY KEY IDENTITY(1,1), NamaToko VARCHAR(100) NOT NULL,
                Alamat VARCHAR(255), Telepon VARCHAR(20))",
            @"IF OBJECT_ID('dbo.Users','U') IS NULL CREATE TABLE dbo.Users (
                IdUser INT PRIMARY KEY IDENTITY(1,1), Username VARCHAR(50) UNIQUE NOT NULL,
                PasswordHash VARCHAR(255) NOT NULL, Role VARCHAR(20) NOT NULL, NamaLengkap VARCHAR(100) NOT NULL)",
            @"IF OBJECT_ID('dbo.Kategori','U') IS NULL CREATE TABLE dbo.Kategori (
                IdKategori INT PRIMARY KEY IDENTITY(1,1), NamaKategori VARCHAR(50) NOT NULL)",
            @"IF OBJECT_ID('dbo.Diskon','U') IS NULL CREATE TABLE dbo.Diskon (
                IdDiskon INT PRIMARY KEY IDENTITY(1,1), NamaDiskon VARCHAR(50) NOT NULL,
                Persentase DECIMAL(5,2) NOT NULL, IsActive BIT DEFAULT 1)",
            @"IF OBJECT_ID('dbo.Pelanggan','U') IS NULL CREATE TABLE dbo.Pelanggan (
                IdPelanggan INT PRIMARY KEY IDENTITY(1,1), NamaPelanggan VARCHAR(100) NOT NULL,
                Kontak VARCHAR(20), Poin INT DEFAULT 0)",
            @"IF OBJECT_ID('dbo.Produk','U') IS NULL CREATE TABLE dbo.Produk (
                IdProduk INT PRIMARY KEY IDENTITY(1,1),
                IdKategori INT FOREIGN KEY REFERENCES dbo.Kategori(IdKategori),
                IdDiskon INT NULL FOREIGN KEY REFERENCES dbo.Diskon(IdDiskon),
                NamaProduk VARCHAR(100) NOT NULL, Harga DECIMAL(12,2) NOT NULL, Stok INT NOT NULL DEFAULT 0)",
            @"IF OBJECT_ID('dbo.Transaksi','U') IS NULL CREATE TABLE dbo.Transaksi (
                IdTransaksi INT PRIMARY KEY IDENTITY(1,1),
                IdUser INT FOREIGN KEY REFERENCES dbo.Users(IdUser),
                IdPelanggan INT NULL FOREIGN KEY REFERENCES dbo.Pelanggan(IdPelanggan),
                WaktuTransaksi DATETIME DEFAULT GETDATE(),
                TotalBayar DECIMAL(12,2) NOT NULL, Kembalian DECIMAL(12,2) NOT NULL)",
            @"IF OBJECT_ID('dbo.DetailTransaksi','U') IS NULL CREATE TABLE dbo.DetailTransaksi (
                IdDetail INT PRIMARY KEY IDENTITY(1,1),
                IdTransaksi INT FOREIGN KEY REFERENCES dbo.Transaksi(IdTransaksi),
                IdProduk INT FOREIGN KEY REFERENCES dbo.Produk(IdProduk),
                Qty INT NOT NULL, HargaSatuan DECIMAL(12,2) NOT NULL, Subtotal DECIMAL(12,2) NOT NULL)",
            @"IF OBJECT_ID('dbo.ActivityLog','U') IS NULL CREATE TABLE dbo.ActivityLog (
                IdLog INT PRIMARY KEY IDENTITY(1,1),
                IdUser INT FOREIGN KEY REFERENCES dbo.Users(IdUser),
                Aksi VARCHAR(255) NOT NULL, Waktu DATETIME DEFAULT GETDATE())"
        };

        public static string Friendly(SqlException ex) => ex.Number switch
        {
            18456 => "Login SQL Server ditolak. Periksa username dan password.",
            4060 => "Akun tidak punya akses ke database tersebut.",
            262 or 229 => "Akun SQL Server tidak punya izin membuat database.",
            -1 or 2 or 53 or 26 or 40 or 10060 or 10061 =>
                "Server tidak ditemukan atau tidak dapat dijangkau. Periksa alamat server dan port, serta pastikan SQL Server berjalan.",
            _ => $"Tidak dapat terhubung ke SQL Server (kode {ex.Number})."
        };

        public (bool Ok, string Message) TestConnection(DbSettings s)
        {
            try
            {
                bool exists;
                using (var conn = new SqlConnection(s.BuildConnectionString("master", 8)))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT DB_ID(@n)", conn))
                    {
                        cmd.Parameters.AddWithValue("@n", s.Database);
                        object? o = cmd.ExecuteScalar();
                        exists = o != null && o != DBNull.Value;
                    }
                }
                if (!exists) return (true, "Koneksi berhasil. Database baru akan dibuat.");

                using (var conn = new SqlConnection(s.BuildConnectionString(s.Database, 8)))
                {
                    conn.Open();
                    const string q = "IF OBJECT_ID('dbo.Users','U') IS NULL SELECT 0 ELSE SELECT COUNT(*) FROM dbo.Users";
                    using (var cmd = new SqlCommand(q, conn))
                    {
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            return (false, $"Database '{s.Database}' sudah berisi akun pengguna (sudah terinstal). Gunakan nama database lain.");
                    }
                }
                return (true, "Koneksi berhasil. Database sudah ada dan akan dipakai.");
            }
            catch (SqlException ex) { return (false, Friendly(ex)); }
            catch (Exception) { return (false, "Pengaturan koneksi tidak valid. Periksa alamat server."); }
        }

        public void Install(DbSettings s, InstallData d, IProgress<string> progress)
        {
            progress.Report("Menyiapkan database...");
            using (var conn = new SqlConnection(s.BuildConnectionString("master", 15)))
            {
                conn.Open();
                const string q = @"IF DB_ID(@n) IS NULL BEGIN
                                     DECLARE @sql nvarchar(300) = N'CREATE DATABASE ' + QUOTENAME(@n);
                                     EXEC(@sql);
                                   END";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@n", s.Database);
                    cmd.ExecuteNonQuery();
                }
            }

            using (var conn = new SqlConnection(s.BuildConnectionString(s.Database, 15)))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    progress.Report("Membuat tabel...");
                    foreach (var sql in SchemaStatements)
                        using (var cmd = new SqlCommand(sql, conn, tx))
                            cmd.ExecuteNonQuery();

                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.Users", conn, tx))
                        if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                            throw new InstallException($"Database '{s.Database}' sudah berisi akun pengguna. Gunakan nama database lain.");

                    progress.Report("Menyimpan akun administrator...");
                    const string insAdmin = @"INSERT INTO dbo.Users (Username, PasswordHash, Role, NamaLengkap)
                                              VALUES (@u, @p, 'Admin', @n)";
                    using (var cmd = new SqlCommand(insAdmin, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@u", d.AdminUser);
                        cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash(d.AdminPassword));
                        cmd.Parameters.AddWithValue("@n", d.AdminNama);
                        cmd.ExecuteNonQuery();
                    }

                    progress.Report("Menyimpan profil toko...");
                    const string insToko = @"
                        IF EXISTS (SELECT 1 FROM dbo.ProfilToko)
                            UPDATE dbo.ProfilToko SET NamaToko = @n, Alamat = @a, Telepon = @t
                        ELSE
                            INSERT INTO dbo.ProfilToko (NamaToko, Alamat, Telepon) VALUES (@n, @a, @t)";
                    using (var cmd = new SqlCommand(insToko, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@n", d.NamaToko);
                        cmd.Parameters.AddWithValue("@a", string.IsNullOrWhiteSpace(d.Alamat) ? DBNull.Value : d.Alamat);
                        cmd.Parameters.AddWithValue("@t", string.IsNullOrWhiteSpace(d.Telepon) ? DBNull.Value : d.Telepon);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }
        }
    }
}