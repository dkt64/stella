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
    public partial class Form_AktywacjaPrzyrzadu : Form
    {
        public Form_AktywacjaPrzyrzadu(string nazwa)
        {
            InitializeComponent();
#if WindowsCE
            this.TopMost = true;
#endif
            label_nazwa.Text = nazwa;
            buttonOK.Focus();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //this.DialogResult = DialogResult.Cancel;
            //this.Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            //this.DialogResult = DialogResult.OK;
            //this.Close();
        }

        private void Form_AktywacjaPrzyrzadu_Closed(object sender, EventArgs e)
        {
            this.TopMost = false;
            //this.DialogResult = DialogResult.Cancel;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //this.DialogResult = DialogResult.OK;
                //this.Close();
            }
        }

        private void textBox1_GotFocus(object sender, EventArgs e)
        {
        }

        private void textBox1_LostFocus(object sender, EventArgs e)
        {
        }

        private void label1_ParentChanged(object sender, EventArgs e)
        {

        }
    }
}