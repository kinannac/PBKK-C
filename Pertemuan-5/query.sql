CREATE DATABASE StudentDB;
GO
USE StudentDB;
GO
CREATE TABLE Students
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NIM VARCHAR(20) NOT NULL,
    Nama VARCHAR(100) NOT NULL,
    Jurusan VARCHAR(100) NOT NULL,
    Gender VARCHAR(20) NOT NULL,
    Email VARCHAR(100)
);

INSERT INTO Students
(NIM, Nama, Jurusan, Gender, Email)
VALUES
('5025241001', 'Budi Santoso', 'Informatika', 'Laki-laki', 'budi@gmail.com'),
('5026241001', 'Siti Aminah', 'Sistem Informasi', 'Perempuan', 'siti@gmail.com'),
('5025241002', 'Andi Wijaya', 'Informatika', 'Laki-laki', 'andi@gmail.com'),
('5026241002', 'Rina Sari', 'Sistem Informasi', 'Perempuan', 'rina@gmail.com');