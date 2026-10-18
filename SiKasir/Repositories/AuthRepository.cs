using Microsoft.Data.SqlClient;
using SiKasir.Helpers;

namespace SiKasir.Repositories
{
    public class AuthRepository
    {
        public string Login(string username, string password, out int loggedInUserId)
        {
            loggedInUserId = 0;
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();

                int id;
                string role, stored;
                using (var cmd = new SqlCommand("SELECT IdUser, Role, PasswordHash FROM Users WHERE Username = @user", conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return string.Empty;
                        id = r.GetInt32(0);
                        role = r.GetString(1);
                        stored = r.GetString(2);
                    }
                }

                bool legacy = !PasswordHasher.IsHashed(stored);
                bool ok = legacy ? stored == password : PasswordHasher.Verify(password, stored);
                if (!ok) return string.Empty;

                // Akun lama dengan password teks biasa di-upgrade ke hash saat login berhasil
                if (legacy)
                {
                    using (var up = new SqlCommand("UPDATE Users SET PasswordHash = @h WHERE IdUser = @id", conn))
                    {
                        up.Parameters.AddWithValue("@h", PasswordHasher.Hash(password));
                        up.Parameters.AddWithValue("@id", id);
                        up.ExecuteNonQuery();
                    }
                }

                loggedInUserId = id;
                return role;
            }
        }
    }
}