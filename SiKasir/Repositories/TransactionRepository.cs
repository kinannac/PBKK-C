using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO;

namespace SiKasir.Repositories
{
    public class ItemKeranjang
    {
        public int IdProduk { get; set; }
        public int Qty { get; set; }
        public decimal HargaSatuan { get; set; }
        public decimal Subtotal => Qty * HargaSatuan;
    }

    public class TransactionRepository
    {
        public bool ProsesCheckout(int idKasir, decimal totalBayar, decimal uangDibayar, List<ItemKeranjang> keranjang)
        {
            decimal kembalian = uangDibayar - totalBayar;
            if (kembalian < 0) return false;

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string insertHeader = "INSERT INTO Transaksi (IdUser, TotalBayar, Kembalian) OUTPUT INSERTED.IdTransaksi VALUES (@idUser, @total, @kembalian)";
                        int idTransaksiBaru;
                        using (var cmd = new SqlCommand(insertHeader, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@idUser", idKasir);
                            cmd.Parameters.AddWithValue("@total", totalBayar);
                            cmd.Parameters.AddWithValue("@kembalian", kembalian);
                            idTransaksiBaru = (int)cmd.ExecuteScalar();
                        }

                        foreach (var item in keranjang)
                        {
                            string insertDetail = "INSERT INTO DetailTransaksi (IdTransaksi, IdProduk, Qty, HargaSatuan, Subtotal) VALUES (@idTrans, @idProd, @qty, @harga, @sub)";
                            using (var cmdDet = new SqlCommand(insertDetail, conn, transaction))
                            {
                                cmdDet.Parameters.AddWithValue("@idTrans", idTransaksiBaru);
                                cmdDet.Parameters.AddWithValue("@idProd", item.IdProduk);
                                cmdDet.Parameters.AddWithValue("@qty", item.Qty);
                                cmdDet.Parameters.AddWithValue("@harga", item.HargaSatuan);
                                cmdDet.Parameters.AddWithValue("@sub", item.Subtotal);
                                cmdDet.ExecuteNonQuery();
                            }

                            string updateStok = "UPDATE Produk SET Stok = Stok - @qty WHERE IdProduk = @idProd";
                            using (var cmdStok = new SqlCommand(updateStok, conn, transaction))
                            {
                                cmdStok.Parameters.AddWithValue("@qty", item.Qty);
                                cmdStok.Parameters.AddWithValue("@idProd", item.IdProduk);
                                cmdStok.ExecuteNonQuery();
                            }
                        }

                        string logQuery = "INSERT INTO ActivityLog (IdUser, Aksi) VALUES (@idUser, 'Melakukan transaksi senilai ' + CAST(@total AS VARCHAR))";
                        using (var cmdLog = new SqlCommand(logQuery, conn, transaction))
                        {
                            cmdLog.Parameters.AddWithValue("@idUser", idKasir);
                            cmdLog.Parameters.AddWithValue("@total", totalBayar);
                            cmdLog.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}