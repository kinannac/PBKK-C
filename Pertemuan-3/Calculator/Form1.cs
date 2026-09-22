using System;
using System.Globalization;
using System.Windows.Forms;

namespace CalculatorApp
{
    public partial class Form1 : Form
    {
        double firstNumber = 0;
        double secondNumber = 0;
        double result = 0;
        string operation = "";
        bool isOperationPerformed = false;

        public Form1()
        {
            InitializeComponent();
        }

        // Event Tombol Angka (Hubungkan btn0 - btn9 ke event ini)
        private void NumberButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            // Jika baru saja menekan tombol operator/sama dengan, bersihkan layar
            if (txtDisplay.Text == "0" || isOperationPerformed)
            {
                txtDisplay.Clear();
                isOperationPerformed = false;
            }

            txtDisplay.Text += button.Text;
        }

        // Event Tombol Operator (Hubungkan tombol +, -, x, / ke event ini)
        private void OperatorButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            if (!string.IsNullOrEmpty(txtDisplay.Text))
            {
                firstNumber = double.Parse(txtDisplay.Text, CultureInfo.InvariantCulture);
                operation = button.Text;
                isOperationPerformed = true;
            }
        }

        // Event Tombol Desimal / Titik (.)
        private void btnDesimal_Click(object sender, EventArgs e)
        {
            if (isOperationPerformed)
            {
                txtDisplay.Text = "0.";
                isOperationPerformed = false;
                return;
            }

            if (!txtDisplay.Text.Contains("."))
            {
                if (string.IsNullOrEmpty(txtDisplay.Text) || txtDisplay.Text == "0")
                {
                    txtDisplay.Text = "0.";
                }
                else
                {
                    txtDisplay.Text += ".";
                }
            }
        }

        // Event Tombol Samadengan (=)
        private void btnSamaDengan_Click(object sender, EventArgs e)
        {
            try
            {
                // Ambil angka kedua dari teks di layar
                secondNumber = Convert.ToDouble(txtDisplay.Text, CultureInfo.InvariantCulture);

                switch (operation)
                {
                    case "+":
                        result = firstNumber + secondNumber;
                        break;
                    case "-":
                        result = firstNumber - secondNumber;
                        break;
                    case "x":
                        result = firstNumber * secondNumber;
                        break;
                    case "/":
                        if (secondNumber == 0)
                        {
                            throw new DivideByZeroException("Tidak dapat membagi dengan nol!");
                        }
                        result = firstNumber / secondNumber;
                        break;
                    default:
                        result = Convert.ToDouble(txtDisplay.Text, CultureInfo.InvariantCulture);
                        break;
                }

                txtDisplay.Text = result.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDisplay.Text = "0";
            }

            isOperationPerformed = true;
        }

        // Event Tombol Clear (C)
        private void btnClear_Click(object sender, EventArgs e)
        {
            firstNumber = 0;
            secondNumber = 0;
            result = 0;
            operation = "";
            isOperationPerformed = false;
            txtDisplay.Text = "0";
        }
    }
}