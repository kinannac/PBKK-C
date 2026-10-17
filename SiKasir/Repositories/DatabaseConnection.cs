using System.IO;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public static class DatabaseConnection
    {
        private static string configPath = "dbconfig.txt";

        public static void SaveConnectionString(string server, string database, string user, string password)
        {
            string connStr = $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
            File.WriteAllText(configPath, connStr);
        }

        public static string GetConnectionString()
        {
            return "Server=tcp:192.168.64.1;Database=SiKasirDB;User Id=sa;Password=SandiKuat2026;TrustServerCertificate=True;";
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }
    }
}

