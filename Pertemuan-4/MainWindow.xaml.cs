using System.Windows;
using System.Windows.Controls;

namespace StudentRegistrationApp
{
    public partial class MainWindow : Window
    {
        private int editingIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSimpan_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            string nim = txtNim.Text;
            string nama = txtNama.Text;
            string prodi = ((ComboBoxItem)cmbProdi.SelectedItem).Content.ToString();
            string jenisKelamin = rbLaki.IsChecked == true ? "Laki-laki" : "Perempuan";

            string data = $"{nim} | {nama} | {prodi} | {jenisKelamin}";

            if (editingIndex == -1)
            {
                lstMahasiswa.Items.Add(data);
                MessageBox.Show("Data mahasiswa berhasil disimpan!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                lstMahasiswa.Items[editingIndex] = data;
                editingIndex = -1;
                MessageBox.Show("Data mahasiswa berhasil diperbarui!", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            ClearForm();
        }

        // Reset Form
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnItemUpdate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                var item = btn.DataContext;
                editingIndex = lstMahasiswa.Items.IndexOf(item);

                if (editingIndex != -1)
                {
                    string rawData = item.ToString();
                    string[] parts = rawData.Split('|');

                    if (parts.Length >= 4)
                    {
                        txtNim.Text = parts[0].Trim();
                        txtNama.Text = parts[1].Trim();

                        string prodi = parts[2].Trim();
                        foreach (ComboBoxItem cbItem in cmbProdi.Items)
                        {
                            if (cbItem.Content.ToString() == prodi)
                            {
                                cmbProdi.SelectedItem = cbItem;
                                break;
                            }
                        }

                        string jk = parts[3].Trim();
                        rbLaki.IsChecked = (jk == "Laki-laki");
                        rbPerempuan.IsChecked = (jk == "Perempuan");
                    }
                }
            }
        }

        private void BtnItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                var item = btn.DataContext;
                MessageBoxResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus data ini?", "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    lstMahasiswa.Items.Remove(item);
                    ClearForm();
                }
            }
        }

        private void ClearForm()
        {
            txtNim.Clear();
            txtNama.Clear();
            cmbProdi.SelectedIndex = -1;
            rbLaki.IsChecked = false;
            rbPerempuan.IsChecked = false;
            editingIndex = -1;
            txtNim.Focus();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtNim.Text))
            {
                MessageBox.Show("NIM harus diisi!");
                txtNim.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama harus diisi!");
                txtNama.Focus();
                return false;
            }
            if (cmbProdi.SelectedItem == null)
            {
                MessageBox.Show("Pilih program studi!");
                return false;
            }
            if (rbLaki.IsChecked != true && rbPerempuan.IsChecked != true)
            {
                MessageBox.Show("Pilih jenis kelamin!");
                return false;
            }
            return true;
        }
    }
}