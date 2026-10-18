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
                UserRepository.EnsureSchema(conn);

                int id;
                string role, stored;
                bool active;
                using (var cmd = new SqlCommand("SELECT IdUser, Role, PasswordHash, IsActive FROM Users WHERE Username = @user", conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return string.Empty;
                        id = r.GetInt32(0);
                        role = r.GetString(1);
                        stored = r.GetString(2);
                        active = r.GetBoolean(3);
                    }
                }

                if (!active) return string.Empty;

                bool legacy = !PasswordHasher.IsHashed(stored);
                bool ok = legacy ? stored == password : PasswordHasher.Verify(password, stored);
                if (!ok) return string.Empty;

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