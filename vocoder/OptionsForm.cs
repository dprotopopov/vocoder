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

        public string AudioFolder
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public double Duration
        {
            get { return (double) numericUpDown3.Value; }
            set { numericUpDown3.Value = (decimal) value; }
        }

        public int Frequency
        {
            get { return (int) numericUpDown2.Value; }
            set { numericUpDown2.Value = value; }
        }

        public int SpectrumLength
        {
            get { return (int) numericUpDown1.Value; }
            set { numericUpDown1.Value = value; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox1.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}