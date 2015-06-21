using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using FFTWSharp;
using NAudio.Wave;
using vocoder.ClassLibrary;
using vocoder.DatabaseLibrary;

namespace vocoder
{
    public partial class DatabaseForm : Form
    {
        public DatabaseForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Database database = MdiParent1.Database;
            listBox1.Items.Clear();
            recordingPanel1.Files.Clear();
            IEnumerable<Record> items = database.Load(new Contact());
            listBox1.Items.AddRange(items.Cast<object>().ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var spectrumBuilder = new SpectrumBuilder(MdiParent1.SpectrumLength, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        spectrumBuilder.Add(waveFileReader);
                }
                var spectrumForm = new ChartForm(spectrumBuilder.GetData());
                spectrumForm.ShowDialog();
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var contact = listBox1.SelectedItem as Contact;
            if (contact == null) return;
            Database database = MdiParent1.Database;
            IEnumerable<Record> items = database.Load(new AudioFile {ContactId = contact.Id});
            recordingPanel1.Files.Clear();
            recordingPanel1.Files.AddRange(items.Select(item => ((AudioFile) item).FileName).ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var contactForm = new ContactForm
            {
                Id = 0,
                FirstName = "",
                LastName = "",
                Phone = "",
                Email = ""
            };
            if (contactForm.ShowDialog() != DialogResult.OK) return;
            Database database = MdiParent1.Database;
            database.InsertOrReplace(new Contact
            {
                Id = contactForm.Id,
                FirstName = contactForm.FirstName,
                LastName = contactForm.LastName,
                Phone = contactForm.Phone,
                Email = contactForm.Email,
            });
            listBox1.Items.Clear();
            recordingPanel1.Files.Clear();
            IEnumerable<Record> items = database.Load(new Contact());
            listBox1.Items.AddRange(items.Cast<object>().ToArray());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var contact = listBox1.SelectedItem as Contact;
            if (contact == null) return;
            var contactForm = new ContactForm
            {
                Id = Convert.ToInt32(contact.Id.ToString()),
                FirstName = contact.FirstName.ToString(),
                LastName = contact.LastName.ToString(),
                Phone = contact.Phone.ToString(),
                Email = contact.Email.ToString()
            };
            if (contactForm.ShowDialog() != DialogResult.OK) return;
            Database database = MdiParent1.Database;
            database.InsertOrReplace(new Contact
            {
                Id = contactForm.Id,
                FirstName = contactForm.FirstName,
                LastName = contactForm.LastName,
                Phone = contactForm.Phone,
                Email = contactForm.Email,
            });
            listBox1.Items.Clear();
            recordingPanel1.Files.Clear();
            IEnumerable<Record> items = database.Load(new Contact());
            listBox1.Items.AddRange(items.Cast<object>().ToArray());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var contact = listBox1.SelectedItem as Contact;
            if (contact == null) return;
            Database database = MdiParent1.Database;
            database.Delete(contact);
            listBox1.Items.Clear();
            recordingPanel1.Files.Clear();
            IEnumerable<Record> items = database.Load(new Contact());
            listBox1.Items.AddRange(items.Cast<object>().ToArray());
        }

        private void recordingPanel1_OnAddAudioFile(object obj, string file)
        {
            var contact = listBox1.SelectedItem as Contact;
            if (contact == null) return;
            Database database = MdiParent1.Database;
            database.InsertOrReplace(new AudioFile {ContactId = contact.Id, FileName = file});
        }

        private void recordingPanel1_OnDeleteAudioFile(object obj, string file)
        {
            var contact = listBox1.SelectedItem as Contact;
            if (contact == null) return;
            Database database = MdiParent1.Database;
            database.Delete(new AudioFile {ContactId = contact.Id, FileName = file});
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        waveBuilder.Add(waveFileReader);
                }
                Complex[] data = waveBuilder.GetData_Complex();
                var soundCorrelations = new List<AudioForm.SoundCorrelation>();
                foreach (var sound in MdiParent1.SoundsClassifier)
                {
                    string phoneme = sound.Key;
                    int count = data.Length;
                    Complex[] complexs = data.Zip(sound.Value, (x, y) => (x*y)).ToArray();
                    var input = new fftw_complexarray(complexs);
                    var output = new fftw_complexarray(count);
                    fftw_plan.dft_1d(count, input, output, fftw_direction.Backward, fftw_flags.Estimate).Execute();
                    List<double> list = output.GetData_Complex().Select(x=>x.Magnitude).ToList();
                    list.Sort();
                    if (list.Count > MdiParent1.SinglePhonemeCount)
                        list.RemoveRange(0, list.Count - MdiParent1.SinglePhonemeCount);
                    soundCorrelations.AddRange(
                        list.Select(value => new AudioForm.SoundCorrelation {Phoneme = phoneme, Value = value}));
                    soundCorrelations.Sort();
                    if (soundCorrelations.Count > MdiParent1.TotalPhonemeCount)
                        soundCorrelations.RemoveRange(MdiParent1.TotalPhonemeCount,
                            soundCorrelations.Count - MdiParent1.TotalPhonemeCount);
                }
                var listBoxForm = new ListBoxForm(soundCorrelations.Select(item => item.Phoneme).Cast<object>(),
                    soundCorrelations.Select(item => item.Value));
                listBoxForm.ShowDialog();
            }
        }
    }
}