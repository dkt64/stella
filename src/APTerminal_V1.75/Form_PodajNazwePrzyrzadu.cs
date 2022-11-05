using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

#if WindowsCE
using Microsoft.WindowsCE.Forms;
#endif

namespace APTerminal
{
    public partial class Form_PodajNazwePrzyrzadu : Form
    {

#if WindowsCE
        InputPanel inputPanel;
#endif
        public Form_PodajNazwePrzyrzadu()
        {
            InitializeComponent();
#if WindowsCE
            inputPanel = new InputPanel();
            this.TopMost = true;
#endif
            buttonOK.Focus();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            textNazwa.Text = "";
            //this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Form_PodajNazwePrzyrzadu_Closed(object sender, EventArgs e)
        {
            this.TopMost = false;
            this.DialogResult = DialogResult.Cancel;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;

                this.Close();
#if WindowsCE
                inputPanel.Enabled = false;
#endif
            }
        }

        private void textBox1_GotFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel.Enabled = true;
#endif
        }

        private void textBox1_LostFocus(object sender, EventArgs e)
        {
#if WindowsCE
            inputPanel.Enabled = false;
#endif
        }
    }
}