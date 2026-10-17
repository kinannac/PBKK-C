using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;

namespace SiKasir.Repositories
{
    public class DashboardRepository
    {
        public (int JumlahTransaksi, decimal TotalBarangTerjual) GetStatistikKasirHariIni(int idKasir)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = @"
                SELECT 
                    COUNT(IdTransaksi) as JmlTransaksi, 
                    COALESCE(SUM(TotalBayar), 0) as TotalPendapatan 
                FROM Transaksi 
                WHERE IdUser = @idUser AND CAST(WaktuTransaksi AS DATE) = CAST(GETDATE() AS DATE)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idUser", idKasir);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return (reader.GetInt32(0), reader.GetDecimal(1));
                        }
                    }
                }
            }
            return (0, 0);
        }

        public decimal GetTotalPendapatanBulanIni()
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT COALESCE(SUM(TotalBayar), 0) FROM Transaksi WHERE MONTH(WaktuTransaksi) = MONTH(GETDATE())";
                using (var cmd = new SqlCommand(query, conn))
                {
                    return (decimal)cmd.ExecuteScalar();
                }
            }
        }

        public DataTable GetProdukStokMenipis(int batasStok = 5)
        {
            DataTable dt = new DataTable();
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string query = "SELECT NamaProduk, Stok FROM Produk WHERE Stok <= @batas";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@batas", batasStok);
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}

    