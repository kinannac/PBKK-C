using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace SiKasir.Repositories
{
    public class ItemKeranjang
    {
        public int IdProduk { get; set; }
        public int Qty { get; set; }
        public decimal HargaSatuan { get; set; }
        public decimal Subtotal => Qty * HargaSatuan;
    }

    public class CheckoutResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public decimal Total { get; init; }
        public decimal Kembalian { get; init; }
        public static CheckoutResult Fail(string message) => new() { Success = false, Message = message };
    }

    public class TransactionRepository
    {
        private static readonly CultureInfo Id = new("id-ID");
        private static string Rp(decimal v) => "Rp " + v.ToString("N0", Id);

        public CheckoutResult ProsesCheckout(int idKasir, decimal uangDibayar, List<ItemKeranjang> keranjang)
        {
            if (keranjang == null || keranjang.Count == 0)
                return CheckoutResult.Fail("Keranjang masih kosong.");
            if (keranjang.Any(k => k.Qty <= 0))
                return CheckoutResult.Fail("Jumlah barang tidak valid.");

            // Gabungkan baris ganda; urutan tetap mencegah deadlock antar kasir
            var items = keranjang
                .GroupBy(k => k.IdProduk)
                .Select(g => new { IdProduk = g.Key, Qty = g.Sum(x => x.Qty), HargaKlien = g.First().HargaSatuan })
                .OrderBy(x => x.IdProduk)
                .ToList();

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        // Return di bawah ini otomatis membatalkan transaksi (rollback saat tx di-dispose)
                        var baris = new List<(int Id, int Qty, decimal Harga)>();
                        decimal total = 0;

                        foreach (var it in items)
                        {
                            string nama;
                            decimal harga;
                            int stok;
                            const string qProduk = "SELECT NamaProduk, Harga, Stok FROM Produk WITH (UPDLOCK, ROWLOCK) WHERE IdProduk = @id";
                            using (var cmd = new SqlCommand(qProduk, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@id", it.IdProduk);
                                using (var r = cmd.ExecuteReader())
                                {
                                    if (!r.Read())
                                        return CheckoutResult.Fail("Ada produk di keranjang yang sudah tidak tersedia.");
                                    nama = r.GetString(0);
                                    harga = r.GetDecimal(1);
                                    stok = r.GetInt32(2);
                                }
                            }

                            if (harga != it.HargaKlien)
                                return CheckoutResult.Fail($"Harga {nama} berubah menjadi {Rp(harga)}. Kosongkan keranjang lalu tambahkan ulang.");
                            if (stok < it.Qty)
                                return CheckoutResult.Fail($"Stok {nama} tidak cukup (tersisa {stok}).");

                            baris.Add((it.IdProduk, it.Qty, harga));
                            total += harga * it.Qty;
                        }

                        if (uangDibayar < total)
                            return CheckoutResult.Fail($"Uang dibayar kurang dari total ({Rp(total)}).");
                        decimal kembalian = uangDibayar - total;

                        int idTransaksi;
                        const string insHeader = @"INSERT INTO Transaksi (IdUser, TotalBayar, Kembalian)
                                                   OUTPUT INSERTED.IdTransaksi VALUES (@u, @t, @k)";
                        using (var cmd = new SqlCommand(insHeader, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", idKasir);
                            cmd.Parameters.AddWithValue("@t", total);
                            cmd.Parameters.AddWithValue("@k", kembalian);
                            idTransaksi = (int)cmd.ExecuteScalar();
                        }

                        foreach (var b in baris)
                        {
                            const string insDetail = @"INSERT INTO DetailTransaksi (IdTransaksi, IdProduk, Qty, HargaSatuan, Subtotal)
                                                       VALUES (@t, @p, @q, @h, @s)";
                            using (var cmd = new SqlCommand(insDetail, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@t", idTransaksi);
                                cmd.Parameters.AddWithValue("@p", b.Id);
                                cmd.Parameters.AddWithValue("@q", b.Qty);
                                cmd.Parameters.AddWithValue("@h", b.Harga);
                                cmd.Parameters.AddWithValue("@s", b.Harga * b.Qty);
                                cmd.ExecuteNonQuery();
                            }

                            const string updStok = "UPDATE Produk SET Stok = Stok - @q WHERE IdProduk = @p AND Stok >= @q";
                            using (var cmd = new SqlCommand(updStok, conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@q", b.Qty);
                                cmd.Parameters.AddWithValue("@p", b.Id);
                                if (cmd.ExecuteNonQuery() != 1)
                                    return CheckoutResult.Fail("Stok berubah saat transaksi diproses. Silakan coba lagi.");
                            }
                        }

                        using (var cmd = new SqlCommand("INSERT INTO ActivityLog (IdUser, Aksi) VALUES (@u, @a)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@u", idKasir);
                            cmd.Parameters.AddWithValue("@a", $"Melakukan transaksi senilai {Rp(total)}");
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                        return new CheckoutResult { Success = true, Total = total, Kembalian = kembalian };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex); // detail teknis hanya untuk developer
                return CheckoutResult.Fail("Transaksi gagal disimpan. Periksa koneksi database lalu coba lagi.");
            }
        }
    }
}