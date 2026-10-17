using System;
using System.Windows;
using SiKasir.Repositories;

namespace SiKasir
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Panggil fungsi test saat aplikasi baru saja menyala
            TestDatabaseConnection();
        }

        private void TestDatabaseConnection()
        {
            try
            {
                // Kita akan mengetes fungsi SetupAwal dari SetupRepository
                var setupRepo = new SetupRepository();

                // Masukkan data dummy untuk testing
                bool hasilSetup = setupRepo.SetupAwal("Toko Mac Maju Jaya", "admin_mac", "rahasia123", "Budi Admin");

                if (hasilSetup)
                {
                    MessageBox.Show("Sukses! Backend berhasil terhubung ke Docker Mac dan tabel berhasil diisi data.", "Test Berhasil");
                }
                else
                {
                    MessageBox.Show("Gagal menyimpan data ke database. Logika kodenya berjalan, tapi ada yang salah di eksekusi SQL.", "Test Gagal");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Sistem/Koneksi: " + ex.Message, "Test Error");
            }
        }
    }
}