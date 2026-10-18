using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public class KategoriDto
    {
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; } = "";
    }

    public class KategoriRepository
    {
        public List<KategoriDto> GetSemua()
        {
            var list = new List<KategoriDto>();
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT IdKategori, NamaKategori FROM Kategori ORDER BY NamaKategori", conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new KategoriDto { IdKategori = r.GetInt32(0), NamaKategori = r.GetString(1) });
                }
            }
            return list;
        }

        public int Tambah(string nama)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string q = "INSERT INTO Kategori (NamaKategori) OUTPUT INSERTED.IdKategori VALUES (@n)";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@n", nama);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}