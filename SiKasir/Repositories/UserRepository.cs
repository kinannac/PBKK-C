using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using SiKasir.Helpers;

namespace SiKasir.Repositories
{
    public class UserDto
    {
        public int IdUser { get; set; }
        public string Username { get; set; } = "";
        public string NamaLengkap { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Aktif" : "Nonaktif";
    }

    public class UserRepository
    {
        // Menambah kolom IsActive jika belum ada (aman dijalankan berulang)
        public static void EnsureSchema(SqlConnection conn)
        {
            const string q = @"IF COL_LENGTH('dbo.Users','IsActive') IS NULL
                ALTER TABLE dbo.Users ADD IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1;";
            using (var cmd = new SqlCommand(q, conn)) cmd.ExecuteNonQuery();
        }

        public List<UserDto> GetAll()
        {
            var list = new List<UserDto>();
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                EnsureSchema(conn);
                const string q = "SELECT IdUser, Username, NamaLengkap, Role, IsActive FROM Users ORDER BY Role, NamaLengkap";
                using (var cmd = new SqlCommand(q, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(new UserDto
                        {
                            IdUser = r.GetInt32(0),
                            Username = r.GetString(1),
                            NamaLengkap = r.GetString(2),
                            Role = r.GetString(3),
                            IsActive = r.GetBoolean(4)
                        });
                }
            }
            return list;
        }

        public void Tambah(string username, string nama, string role, string password)
        {
            if (role != "Kasir" && role != "Admin")
                throw new ArgumentException("Peran tidak valid.");

            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                EnsureSchema(conn);
                const string q = @"INSERT INTO Users (Username, PasswordHash, Role, NamaLengkap)
                                   VALUES (@u, @p, @r, @n)";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash(password));
                    cmd.Parameters.AddWithValue("@r", role);
                    cmd.Parameters.AddWithValue("@n", nama);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ResetPassword(int idUser, string password)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("UPDATE Users SET PasswordHash = @p WHERE IdUser = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash(password));
                    cmd.Parameters.AddWithValue("@id", idUser);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Admin aktif terakhir tidak boleh dinonaktifkan
        public void SetAktif(int idUser, bool aktif)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                EnsureSchema(conn);
                const string q = @"UPDATE Users SET IsActive = @a
                    WHERE IdUser = @id
                      AND (@a = 1 OR Role <> 'Admin'
                           OR (SELECT COUNT(*) FROM Users WHERE Role = 'Admin' AND IsActive = 1) > 1)";
                using (var cmd = new SqlCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@a", aktif);
                    cmd.Parameters.AddWithValue("@id", idUser);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Admin aktif terakhir tidak bisa dinonaktifkan.");
                }
            }
        }
    }
}