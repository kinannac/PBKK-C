class Program
{
    // List untuk menyimpan data mahasiswa sementara
    static List<string> daftarMahasiswa = new List<string>();

    static void Main(string[] args)
    {
        bool berjalan = true;

        while (berjalan)
        {
            Console.Clear();
            Console.WriteLine("WELCOME TO MINI SIAKAD");
            Console.WriteLine("1. Tambahkan data mahasiswa");
            Console.WriteLine("2. Tampilkan data mahasiswa");
            Console.WriteLine("3. Cari data mahasiswa");
            Console.WriteLine("4. Hapus data mahasiswa");
            Console.WriteLine("5. Keluar Program");
            Console.Write("Pilih menu (1-5): ");

            string pilihan = Console.ReadLine();

            switch (pilihan)
            {
                case "1":
                    TambahMahasiswa();
                    break;
                case "2":
                    TampilkanMahasiswa();
                    break;
                case "3":
                    CariMahasiswa();
                    break;
                case "4":
                    HapusMahasiswa();
                    break;
                case "5":
                    berjalan = false;
                    Console.WriteLine("\nTerima kasih telah menggunakan Mini SIAKAD!");
                    break;
                default:
                    Console.WriteLine("\nPilihan tidak valid! Tekan Enter untuk coba lagi.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static void TambahMahasiswa()
    {
        Console.WriteLine("\nTAMBAH MAHASISWA");
        Console.Write("Masukkan Nama/NRP: ");
        string nama = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(nama))
        {
            daftarMahasiswa.Add(nama);
            Console.WriteLine("Data berhasil ditambahkan!");
        }
        else
        {
            Console.WriteLine("Nama tidak boleh kosong!");
        }

        Pause();
    }

    static void TampilkanMahasiswa()
    {
        Console.WriteLine("\nDAFTAR MAHASISWA");
        if (daftarMahasiswa.Count == 0)
        {
            Console.WriteLine("Belum ada data mahasiswa.");
        }
        else
        {
            for (int i = 0; i < daftarMahasiswa.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {daftarMahasiswa[i]}");
            }
        }

        Pause();
    }

    static void CariMahasiswa()
    {
        Console.WriteLine("\nCARI MAHASISWA");
        Console.Write("Masukkan nama/keyword yang dicari: ");
        string cari = Console.ReadLine();

        // Cari data yang mengandung kata kunci (case-insensitive)
        var hasil = daftarMahasiswa.FindAll(m => m.Contains(cari, StringComparison.OrdinalIgnoreCase));

        if (hasil.Count > 0)
        {
            Console.WriteLine("\nData ditemukan:");
            foreach (var item in hasil)
            {
                Console.WriteLine($"- {item}");
            }
        }
        else
        {
            Console.WriteLine("Data tidak ditemukan.");
        }

        Pause();
    }

    static void HapusMahasiswa()
    {
        Console.WriteLine("\nHAPUS MAHASISWA");
        TampilkanMahasiswaRingkas();

        if (daftarMahasiswa.Count > 0)
        {
            Console.Write("\nMasukkan nomor data yang ingin dihapus: ");
            if (int.TryParse(Console.ReadLine(), out int nomor) && nomor > 0 && nomor <= daftarMahasiswa.Count)
            {
                string dihapus = daftarMahasiswa[nomor - 1];
                daftarMahasiswa.RemoveAt(nomor - 1);
                Console.WriteLine($"Berhasil menghapus: {dihapus}");
            }
            else
            {
                Console.WriteLine("Nomor tidak valid!");
            }
        }

        Pause();
    }

    static void TampilkanMahasiswaRingkas()
    {
        for (int i = 0; i < daftarMahasiswa.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {daftarMahasiswa[i]}");
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nTekan Enter untuk kembali ke menu utama...");
        Console.ReadLine();
    }
}
