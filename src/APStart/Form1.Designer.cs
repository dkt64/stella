namespace APStart
{
    partial class Form1
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonChange = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.labelTimer = new System.Windows.Forms.Label();
            this.timerStart = new System.Windows.Forms.Timer();
            this.timerClose = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.BackColor = System.Drawing.Color.Black;
            this.buttonCancel.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
            this.buttonCancel.ForeColor = System.Drawing.Color.Violet;
            this.buttonCancel.Location = new System.Drawing.Point(288, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(241, 63);
            this.buttonCancel.TabIndex = 0;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.label1.ForeColor = System.Drawing.Color.Violet;
            this.label1.Location = new System.Drawing.Point(63, 182);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 20);
            this.label1.Text = "Path:";
            // 
            // labelPath
            // 
            this.labelPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelPath.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.labelPath.ForeColor = System.Drawing.Color.Violet;
            this.labelPath.Location = new System.Drawing.Point(124, 182);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(440, 20);
            this.labelPath.Text = "\\StorageCard\\APWELD\\APTerminal\\APTerminal.exe";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Tahoma", 36F, System.Drawing.FontStyle.Regular);
            this.label3.ForeColor = System.Drawing.Color.Violet;
            this.label3.Location = new System.Drawing.Point(63, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(674, 69);
            this.label3.Text = "APTerminal starter application";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonChange
            // 
            this.buttonChange.BackColor = System.Drawing.Color.Black;
            this.buttonChange.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
            this.buttonChange.ForeColor = System.Drawing.Color.Violet;
            this.buttonChange.Location = new System.Drawing.Point(598, 167);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.Size = new System.Drawing.Size(139, 47);
            this.buttonChange.TabIndex = 6;
            this.buttonChange.Text = "Change ...";
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
            this.label4.ForeColor = System.Drawing.Color.Violet;
            this.label4.Location = new System.Drawing.Point(315, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(182, 28);
            this.label4.Text = "for Windows CE";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.BackColor = System.Drawing.Color.Black;
            this.buttonStartStop.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
            this.buttonStartStop.ForeColor = System.Drawing.Color.Violet;
            this.buttonStartStop.Location = new System.Drawing.Point(288, 341);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new System.Drawing.Size(241, 63);
            this.buttonStartStop.TabIndex = 9;
            this.buttonStartStop.Text = "Stop";
            this.buttonStartStop.Click += new System.EventHandler(this.buttonStartStop_Click);
            // 
            // labelTimer
            // 
            this.labelTimer.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Regular);
            this.labelTimer.ForeColor = System.Drawing.Color.White;
            this.labelTimer.Location = new System.Drawing.Point(259, 121);
            this.labelTimer.Name = "labelTimer";
            this.labelTimer.Size = new System.Drawing.Size(296, 28);
            this.labelTimer.Text = "Start in 0 seconds";
            this.labelTimer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // timerStart
            // 
            this.timerStart.Interval = 1000;
            this.timerStart.Tick += new System.EventHandler(this.timerStart_Tick);
            // 
            // timerClose
            // 
            this.timerClose.Interval = 30000;
            this.timerClose.Tick += new System.EventHandler(this.timerClose_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(800, 480);
            this.ControlBox = false;
            this.Controls.Add(this.labelTimer);
            this.Controls.Add(this.buttonStartStop);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "APStart";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonStartStop;
        private System.Windows.Forms.Label labelTimer;
        private System.Windows.Forms.Timer timerStart;
        private System.Windows.Forms.Timer timerClose;
    }
}

