namespace CalculatorApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtDisplay = new TextBox();
            btn8 = new Button();
            btn7 = new Button();
            btn9 = new Button();
            btn6 = new Button();
            btn4 = new Button();
            btn5 = new Button();
            btn3 = new Button();
            btn1 = new Button();
            btn2 = new Button();
            btnSamaDengan = new Button();
            btnClear = new Button();
            btn0 = new Button();
            btnTambah = new Button();
            btnKurang = new Button();
            btnKali = new Button();
            btnBagi = new Button();
            btnDesimal = new Button();
            SuspendLayout();
            // 
            // txtDisplay
            // 
            txtDisplay.Location = new Point(12, 12);
            txtDisplay.Name = "txtDisplay";
            txtDisplay.Size = new Size(336, 23);
            txtDisplay.TabIndex = 0;
            txtDisplay.Text = "0";
            // 
            // btn8
            // 
            btn8.Location = new Point(102, 106);
            btn8.Name = "btn8";
            btn8.Size = new Size(79, 36);
            btn8.TabIndex = 1;
            btn8.Text = "8";
            btn8.UseVisualStyleBackColor = true;
            btn8.Click += NumberButton_Click;
            // 
            // btn7
            // 
            btn7.Location = new Point(12, 106);
            btn7.Name = "btn7";
            btn7.Size = new Size(84, 36);
            btn7.TabIndex = 2;
            btn7.Text = "7";
            btn7.UseVisualStyleBackColor = true;
            btn7.Click += NumberButton_Click;
            // 
            // btn9
            // 
            btn9.Location = new Point(187, 106);
            btn9.Name = "btn9";
            btn9.Size = new Size(79, 36);
            btn9.TabIndex = 3;
            btn9.Text = "9";
            btn9.UseVisualStyleBackColor = true;
            btn9.Click += NumberButton_Click;
            // 
            // btn6
            // 
            btn6.Location = new Point(187, 161);
            btn6.Name = "btn6";
            btn6.Size = new Size(79, 36);
            btn6.TabIndex = 6;
            btn6.Text = "6";
            btn6.UseVisualStyleBackColor = true;
            btn6.Click += NumberButton_Click;
            // 
            // btn4
            // 
            btn4.Location = new Point(12, 161);
            btn4.Name = "btn4";
            btn4.Size = new Size(84, 36);
            btn4.TabIndex = 5;
            btn4.Text = "4";
            btn4.UseVisualStyleBackColor = true;
            btn4.Click += NumberButton_Click;
            // 
            // btn5
            // 
            btn5.Location = new Point(102, 161);
            btn5.Name = "btn5";
            btn5.Size = new Size(79, 36);
            btn5.TabIndex = 4;
            btn5.Text = "5";
            btn5.UseVisualStyleBackColor = true;
            btn5.Click += NumberButton_Click;
            // 
            // btn3
            // 
            btn3.Location = new Point(187, 218);
            btn3.Name = "btn3";
            btn3.Size = new Size(79, 36);
            btn3.TabIndex = 9;
            btn3.Text = "3";
            btn3.UseVisualStyleBackColor = true;
            btn3.Click += NumberButton_Click;
            // 
            // btn1
            // 
            btn1.Location = new Point(12, 218);
            btn1.Name = "btn1";
            btn1.Size = new Size(84, 36);
            btn1.TabIndex = 8;
            btn1.Text = "1";
            btn1.UseVisualStyleBackColor = true;
            btn1.Click += NumberButton_Click;
            // 
            // btn2
            // 
            btn2.Location = new Point(102, 218);
            btn2.Name = "btn2";
            btn2.Size = new Size(79, 36);
            btn2.TabIndex = 7;
            btn2.Text = "2";
            btn2.UseVisualStyleBackColor = true;
            btn2.Click += NumberButton_Click;
            // 
            // btnSamaDengan
            // 
            btnSamaDengan.Location = new Point(285, 276);
            btnSamaDengan.Name = "btnSamaDengan";
            btnSamaDengan.Size = new Size(63, 36);
            btnSamaDengan.TabIndex = 12;
            btnSamaDengan.Text = "=";
            btnSamaDengan.UseVisualStyleBackColor = true;
            btnSamaDengan.Click += btnSamaDengan_Click;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(12, 276);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(84, 36);
            btnClear.TabIndex = 11;
            btnClear.Text = "C";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btn0
            // 
            btn0.Location = new Point(102, 276);
            btn0.Name = "btn0";
            btn0.Size = new Size(79, 36);
            btn0.TabIndex = 10;
            btn0.Text = "0";
            btn0.UseVisualStyleBackColor = true;
            btn0.Click += NumberButton_Click;
            // 
            // btnTambah
            // 
            btnTambah.Location = new Point(285, 235);
            btnTambah.Name = "btnTambah";
            btnTambah.Size = new Size(63, 36);
            btnTambah.TabIndex = 16;
            btnTambah.Text = "+";
            btnTambah.UseVisualStyleBackColor = true;
            btnTambah.Click += OperatorButton_Click;
            // 
            // btnKurang
            // 
            btnKurang.Location = new Point(285, 192);
            btnKurang.Name = "btnKurang";
            btnKurang.Size = new Size(63, 36);
            btnKurang.TabIndex = 15;
            btnKurang.Text = "-";
            btnKurang.UseVisualStyleBackColor = true;
            btnKurang.Click += OperatorButton_Click;
            // 
            // btnKali
            // 
            btnKali.Location = new Point(285, 149);
            btnKali.Name = "btnKali";
            btnKali.Size = new Size(63, 36);
            btnKali.TabIndex = 14;
            btnKali.Text = "x";
            btnKali.UseVisualStyleBackColor = true;
            btnKali.Click += OperatorButton_Click;
            // 
            // btnBagi
            // 
            btnBagi.Location = new Point(285, 106);
            btnBagi.Name = "btnBagi";
            btnBagi.Size = new Size(63, 36);
            btnBagi.TabIndex = 13;
            btnBagi.Text = "/";
            btnBagi.UseVisualStyleBackColor = true;
            btnBagi.Click += OperatorButton_Click;
            // 
            // btnDesimal
            // 
            btnDesimal.Location = new Point(187, 276);
            btnDesimal.Name = "btnDesimal";
            btnDesimal.Size = new Size(79, 36);
            btnDesimal.TabIndex = 17;
            btnDesimal.Text = ".";
            btnDesimal.UseVisualStyleBackColor = true;
            btnDesimal.Click += btnDesimal_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(361, 333);
            Controls.Add(btnDesimal);
            Controls.Add(btnTambah);
            Controls.Add(btnKurang);
            Controls.Add(btnKali);
            Controls.Add(btnBagi);
            Controls.Add(btnSamaDengan);
            Controls.Add(btnClear);
            Controls.Add(btn0);
            Controls.Add(btn3);
            Controls.Add(btn1);
            Controls.Add(btn2);
            Controls.Add(btn6);
            Controls.Add(btn4);
            Controls.Add(btn5);
            Controls.Add(btn9);
            Controls.Add(btn7);
            Controls.Add(btn8);
            Controls.Add(txtDisplay);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtDisplay;
        private Button btn8;
        private Button btn7;
        private Button btn9;
        private Button btn6;
        private Button btn4;
        private Button btn5;
        private Button btn3;
        private Button btn1;
        private Button btn2;
        private Button btnSamaDengan;
        private Button btnClear;
        private Button btn0;
        private Button btnTambah;
        private Button btnKurang;
        private Button btnKali;
        private Button btnBagi;
        private Button btnDesimal;
    }
}
