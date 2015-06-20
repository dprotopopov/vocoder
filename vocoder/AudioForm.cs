using System;
using System.Windows.Forms;
using NAudio.Wave;
using vocoder.ClassLibrary;

namespace vocoder
{
    public partial class AudioForm : Form
    {
        public AudioForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var sb = new SpectrumBuilder(MdiParent1.Length, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var audioFileReader = new AudioFileReader(file))
                        sb.Add(audioFileReader);
                }
                var spectrumForm = new SpectrumForm(sb.Normalize());
                spectrumForm.ShowDialog();
            }
        }
    }
}