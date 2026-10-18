using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public static class DatabaseConnection
    {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SiKasir");
        private static readonly string ConfigPath = Path.Combine(ConfigDir, "dbconfig.txt");
        private static string? _cached;

        public static bool IsConfigured =>
            File.Exists(ConfigPath) && !string.IsNullOrWhiteSpace(File.ReadAllText(ConfigPath));

        public static void SaveConnectionString(string connectionString)
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigPath, connectionString);
            _cached = connectionString;
        }

        // Dipertahankan agar kode lama tetap kompatibel
        public static void SaveConnectionString(string server, string database, string user, string password)
            => SaveConnectionString($"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;");

        public static string GetConnectionString()
        {
            if (_cached != null) return _cached;
            if (!IsConfigured)
                throw new InvalidOperationException("Aplikasi belum dikonfigurasi. Jalankan installer terlebih dahulu.");
            _cached = File.ReadAllText(ConfigPath).Trim();
            return _cached;
        }

        public static SqlConnection GetConnection() => new SqlConnection(GetConnectionString());
    }
}