using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public class ProdukDto
    {
        public int IdProduk { get; set; }
        public string NamaProduk { get; set; } = "";
        public decimal Harga { get; set; }
        public int Stok { get; set; }
        public int IdKategori { get; set; }
        public string NamaKategori { get; set; } = "";
    }

    public class ProdukRepository
    {
        private const string SelectSql = @"
            SELECT p.IdProduk, p.NamaProduk, p.Harga, p.Stok,
                   ISNULL(p.IdKategori, 0), ISNULL(k.NamaKategori, '-')
            FROM Produk p
            LEFT JOIN Kategori k ON k.IdKategori = p.IdKategori
            WHERE p.NamaProduk LIKE @k
            ORDER BY p.NamaProduk";

        public List<ProdukDto> Cari(string keyword)
        {
            var list = new List<ProdukDto>();
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(SelectSql, conn))
                {
                    cmd.Parameters.AddWithValue("@k", "%" + keyword + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            list.Add(new ProdukDto
                            {
                                IdProduk = r.GetInt32(0),
                                NamaProduk = r.GetString(1),
                                Harga = r.GetDecimal(2),
                                Stok = r.GetInt32(3),
                                IdKategori = r.GetInt32(4),
                                NamaKategori = r.GetString(5)
                            });
                    }
                }
            }
            return list;
        }

        public void Tambah(ProdukDto p)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string q = "INSERT INTO Produk (IdKategori, NamaProduk, Harga, Stok) VALUES (@kat, @nama, @harga, @stok)";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@kat", p.IdKategori);
                    cmd.Parameters.AddWithValue("@nama", p.NamaProduk);
                    cmd.Parameters.AddWithValue("@harga", p.Harga);
                    cmd.Parameters.AddWithValue("@stok", p.Stok);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Ubah(ProdukDto p)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                string q = "UPDATE Produk SET IdKategori=@kat, NamaProduk=@nama, Harga=@harga, Stok=@stok WHERE IdProduk=@id";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@id", p.IdProduk);
                    cmd.Parameters.AddWithValue("@kat", p.IdKategori);
                    cmd.Parameters.AddWithValue("@nama", p.NamaProduk);
                    cmd.Parameters.AddWithValue("@harga", p.Harga);
                    cmd.Parameters.AddWithValue("@stok", p.Stok);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Hapus(int idProduk)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM Produk WHERE IdProduk=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idProduk);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}