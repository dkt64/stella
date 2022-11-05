namespace APTerminal
{
    partial class Form_PodajNazwePrzyrzadu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.textNazwa = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numeric_Kod = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Regular);
            this.buttonOK.Location = new System.Drawing.Point(180, 104);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(123, 43);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Regular);
            this.buttonCancel.Location = new System.Drawing.Point(309, 104);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(123, 43);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Anuluj";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // textNazwa
            // 
            this.textNazwa.Font = new System.Drawing.Font("Tahoma", 24F, System.Drawing.FontStyle.Regular);
            this.textNazwa.Location = new System.Drawing.Point(13, 50);
            this.textNazwa.Name = "textNazwa";
            this.textNazwa.Size = new System.Drawing.Size(266, 45);
            this.textNazwa.TabIndex = 2;
            this.textNazwa.GotFocus += new System.EventHandler(this.textBox1_GotFocus);
            this.textNazwa.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textNazwa.LostFocus += new System.EventHandler(this.textBox1_LostFocus);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Regular);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 29);
            this.label1.Text = "Nazwa:";
            // 
            // numeric_Kod
            // 
            this.numeric_Kod.Font = new System.Drawing.Font("Tahoma", 23F, System.Drawing.FontStyle.Regular);
            this.numeric_Kod.Location = new System.Drawing.Point(292, 49);
            this.numeric_Kod.Maximum = new decimal(new int[] {
            1023,
            0,
            0,
            0});
            this.numeric_Kod.Name = "numeric_Kod";
            this.numeric_Kod.Size = new System.Drawing.Size(140, 45);
            this.numeric_Kod.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Tahoma", 20F, System.Drawing.FontStyle.Regular);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(292, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(140, 29);
            this.label2.Text = "Kod:";
            // 
            // Form_PodajNazwePrzyrzadu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.RoyalBlue;
            this.ClientSize = new System.Drawing.Size(448, 162);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numeric_Kod);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textNazwa);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_PodajNazwePrzyrzadu";
            this.Text = "Nowy przyrząd";
            this.Closed += new System.EventHandler(this.Form_PodajNazwePrzyrzadu_Closed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textNazwa;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.NumericUpDown numeric_Kod;
    }
}