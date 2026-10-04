using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using StudentManager.Models;

namespace StudentManager.Data
{
    public class StudentRepository
    {
        private readonly string connectionString =
            @"Server=tcp:192.168.64.1;Database=master;User Id=sa;Password=SandiKuat2026;TrustServerCertificate=True;";

        public StudentRepository()
        {
            EnsureDatabaseCreated();
        }

        private void EnsureDatabaseCreated()
        {
            try
            {
                string masterConnectionString = @"Server=tcp:192.168.64.1;Database=master;User Id=sa;Password=SandiKuat2026;TrustServerCertificate=True;";
                using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
                {
                    masterConn.Open();
                    string createDbSql = @"
                        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StudentDB')
                        BEGIN
                            CREATE DATABASE StudentDB;
                        END";
                    using SqlCommand cmd = new SqlCommand(createDbSql, masterConn);
                    cmd.ExecuteNonQuery();
                }

                using (SqlConnection dbConn = new SqlConnection(connectionString))
                {
                    dbConn.Open();
                    string createTableSql = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Students')
                        BEGIN
                            CREATE TABLE Students (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                NIM VARCHAR(20) NOT NULL,
                                Nama VARCHAR(100) NOT NULL,
                                Jurusan VARCHAR(100) NOT NULL,
                                Gender VARCHAR(20) NOT NULL,
                                Email VARCHAR(100)
                            );

                            INSERT INTO Students (NIM, Nama, Jurusan, Gender, Email)
                            VALUES 
                            ('5025241001', 'Budi Santoso', 'Informatika', 'Laki-laki', 'budi@gmail.com'),
                            ('5026241001', 'Siti Aminah', 'Sistem Informasi', 'Perempuan', 'siti@gmail.com'),
                            ('5025241002', 'Andi Wijaya', 'Informatika', 'Laki-laki', 'andi@gmail.com'),
                            ('5026241002', 'Rina Sari', 'Sistem Informasi', 'Perempuan', 'rina@gmail.com');
                        END";
                    using SqlCommand cmd = new SqlCommand(createTableSql, dbConn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Gagal menginisialisasi database: " + ex.Message);
            }
        }

        // READ
        public List<Student> GetAll()
        {
            var students = new List<Student>();
            using SqlConnection connection = new SqlConnection(connectionString);
            string sql = "SELECT Id, NIM, Nama, Jurusan, Gender, Email FROM Students ORDER BY Id DESC";

            using SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NIM = reader["NIM"].ToString()!,
                    Nama = reader["Nama"].ToString()!,
                    Jurusan = reader["Jurusan"].ToString()!,
                    Gender = reader["Gender"].ToString()!,
                    Email = reader["Email"].ToString()!
                });
            }
            return students;
        }

        // INSERT
        public void Insert(Student student)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            string sql = "INSERT INTO Students (NIM, Nama, Jurusan, Gender, Email) VALUES (@NIM, @Nama, @Jurusan, @Gender, @Email)";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@NIM", student.NIM);
            command.Parameters.AddWithValue("@Nama", student.Nama);
            command.Parameters.AddWithValue("@Jurusan", student.Jurusan);
            command.Parameters.AddWithValue("@Gender", student.Gender);
            command.Parameters.AddWithValue("@Email", student.Email);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // UPDATE
        public void Update(Student student)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            string sql = "UPDATE Students SET NIM=@NIM, Nama=@Nama, Jurusan=@Jurusan, Gender=@Gender, Email=@Email WHERE Id=@Id";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Id", student.Id);
            command.Parameters.AddWithValue("@NIM", student.NIM);
            command.Parameters.AddWithValue("@Nama", student.Nama);
            command.Parameters.AddWithValue("@Jurusan", student.Jurusan);
            command.Parameters.AddWithValue("@Gender", student.Gender);
            command.Parameters.AddWithValue("@Email", student.Email);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // DELETE
        public void Delete(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            using SqlCommand command = new SqlCommand("DELETE FROM Students WHERE Id=@Id", connection);
            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        // SEARCH
        public List<Student> Search(string keyword)
        {
            var students = new List<Student>();
            using SqlConnection connection = new SqlConnection(connectionString);
            string sql = "SELECT Id, NIM, Nama, Jurusan, Gender, Email FROM Students WHERE NIM LIKE @Keyword OR Nama LIKE @Keyword OR Jurusan LIKE @Keyword ORDER BY Id DESC";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                students.Add(new Student
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    NIM = reader["NIM"].ToString()!,
                    Nama = reader["Nama"].ToString()!,
                    Jurusan = reader["Jurusan"].ToString()!,
                    Gender = reader["Gender"].ToString()!,
                    Email = reader["Email"].ToString()!
                });
            }
            return students;
        }
    }
}