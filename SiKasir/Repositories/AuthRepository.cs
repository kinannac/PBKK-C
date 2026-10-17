using Microsoft.Data.SqlClient;
using System.IO;

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
                string query = "SELECT IdUser, Role FROM Users WHERE Username = @user AND PasswordHash = @pass";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user", username);
                    cmd.Parameters.AddWithValue("@pass", password);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            loggedInUserId = reader.GetInt32(0);
                            return reader.GetString(1);
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}

