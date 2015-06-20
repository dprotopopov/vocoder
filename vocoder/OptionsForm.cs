using System;
using System.Windows.Forms;

namespace vocoder
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
        }

        public int Frequency
        {
            get { return (int) numericUpDown2.Value; }
            set { numericUpDown2.Value = value; }
        }

        public int Length
        {
            get { return (int) numericUpDown1.Value; }
            set { numericUpDown1.Value = value; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}