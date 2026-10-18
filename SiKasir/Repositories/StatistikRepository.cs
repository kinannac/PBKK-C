using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public class RingkasanAdmin
    {
        public decimal PendapatanBulanIni { get; set; }
        public int TotalProduk { get; set; }
        public int TransaksiBulanIni { get; set; }
        public int TotalPelanggan { get; set; }
    }

    public class PenjualanHarian
    {
        public DateTime Tanggal { get; set; }
        public decimal Total { get; set; }
    }

    public class AktivitasDto
    {
        public string NamaUser { get; set; } = "";
        public string Aksi { get; set; } = "";
        public DateTime Waktu { get; set; }
    }

    public class StatistikRepository
    {
        public RingkasanAdmin GetRingkasanAdmin()
        {
            const string q = @"
                SELECT
                  (SELECT COALESCE(SUM(TotalBayar), 0) FROM Transaksi
                    WHERE YEAR(WaktuTransaksi) = YEAR(GETDATE()) AND MONTH(WaktuTransaksi) = MONTH(GETDATE())),
                  (SELECT COUNT(*) FROM Produk),
                  (SELECT COUNT(*) FROM Transaksi
                    WHERE YEAR(WaktuTransaksi) = YEAR(GETDATE()) AND MONTH(WaktuTransaksi) = MONTH(GETDATE())),
                     (SELECT COUNT(*) FROM Transaksi)";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(q, conn))
                using (var r = cmd.ExecuteReader())
                {
                    var hasil = new RingkasanAdmin();
                    if (r.Read())
                    {
                        hasil.PendapatanBulanIni = r.GetDecimal(0);
                        hasil.TotalProduk = r.GetInt32(1);
                        hasil.TransaksiBulanIni = r.GetInt32(2);
                        hasil.TotalPelanggan = r.GetInt32(3);
                    }
                    return hasil;
                }
            }
        }

        // 7 hari terakhir (termasuk hari ini); hari tanpa transaksi diisi 0
        public List<PenjualanHarian> GetPenjualan7Hari()
        {
            var perHari = new Dictionary<DateTime, decimal>();
            const string q = @"
                SELECT CAST(WaktuTransaksi AS DATE), SUM(TotalBayar)
                FROM Transaksi
                WHERE WaktuTransaksi >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE))
                GROUP BY CAST(WaktuTransaksi AS DATE)";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(q, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        perHari[r.GetDateTime(0).Date] = r.GetDecimal(1);
                }
            }

            var hasil = new List<PenjualanHarian>();
            for (int i = 6; i >= 0; i--)
            {
                var tgl = DateTime.Today.AddDays(-i);
                hasil.Add(new PenjualanHarian
                {
                    Tanggal = tgl,
                    Total = perHari.TryGetValue(tgl, out var t) ? t : 0
                });
            }
            return hasil;
        }

        public List<AktivitasDto> GetAktivitasKasirTerbaru(int jumlah = 10)
        {
            var list = new List<AktivitasDto>();
            const string q = @"
                SELECT TOP (@n) u.NamaLengkap, a.Aksi, a.Waktu
                FROM ActivityLog a
                JOIN Users u ON u.IdUser = a.IdUser
                WHERE u.Role = 'Kasir'
                ORDER BY a.Waktu DESC";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@n", jumlah);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            list.Add(new AktivitasDto
                            {
                                NamaUser = r.GetString(0),
                                Aksi = r.GetString(1),
                                Waktu = r.GetDateTime(2)
                            });
                    }
                }
            }
            return list;
        }

        // Perbaikan: total barang = SUM(Qty), bukan SUM(TotalBayar)
        public (int JumlahTransaksi, int TotalBarang) GetStatistikKasirHariIni(int idKasir)
        {
            const string qTrx = @"
                SELECT COUNT(*) FROM Transaksi
                WHERE IdUser = @id AND CAST(WaktuTransaksi AS DATE) = CAST(GETDATE() AS DATE)";
            const string qBarang = @"
                SELECT COALESCE(SUM(d.Qty), 0)
                FROM DetailTransaksi d
                JOIN Transaksi t ON t.IdTransaksi = d.IdTransaksi
                WHERE t.IdUser = @id AND CAST(t.WaktuTransaksi AS DATE) = CAST(GETDATE() AS DATE)";

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                int trx, barang;
                using (var cmd = new SqlCommand(qTrx, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idKasir);
                    trx = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new SqlCommand(qBarang, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idKasir);
                    barang = Convert.ToInt32(cmd.ExecuteScalar());
                }
                return (trx, barang);
            }
        }
    }
}