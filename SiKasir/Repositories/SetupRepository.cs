using Microsoft.Data.SqlClient;
using System.IO;

namespace SiKasir.Repositories
{
    public class SetupRepository
    {
        public bool SetupAwal(string namaToko, string adminUser, string adminPass, string adminNama)
        {
            using (var conn = DatabaseConnection.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string queryToko = "INSERT INTO ProfilToko (NamaToko) VALUES (@nama)";
                        using (var cmd = new SqlCommand(queryToko, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@nama", namaToko);
                            cmd.ExecuteNonQuery();
                        }

                        string queryAdmin = "INSERT INTO Users (Username, PasswordHash, Role, NamaLengkap) VALUES (@user, @pass, 'Admin', @namaLengkap)";
                        using (var cmd = new SqlCommand(queryAdmin, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@user", adminUser);
                            cmd.Parameters.AddWithValue("@pass", adminPass);
                            cmd.Parameters.AddWithValue("@namaLengkap", adminNama);
                            cmd.ExecuteNonQuery();
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

    