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
                double[] data = waveBuilder.GetData().ToArray();
                var list = new List<AudioForm.SoundCorrelation>();
                foreach (var sound in MdiParent1.SoundsClassifier)
                {
                    string word = sound.Key;
                    var fftw = new fftw_complexarray(data.Zip(sound.Value, (x, y) => (Complex) (x*y)).ToArray());
                    List<double> array = fftw.GetData_Real().ToList();
                    array.Sort();
                    if (array.Count > MdiParent1.SinglePhonemeCount)
                        array.RemoveRange(0, array.Count - MdiParent1.SinglePhonemeCount);
                    list.AddRange(
                        array.Select(value => new AudioForm.SoundCorrelation {Phoneme = word, Value = value}));
                    list.Sort();
                    if (list.Count > MdiParent1.TotalPhonemeCount)
                        list.RemoveRange(MdiParent1.TotalPhonemeCount, list.Count - MdiParent1.TotalPhonemeCount);
                }
                var listBoxForm = new ListBoxForm(list.Select(item => item.Phoneme).Cast<object>(),
                    list.Select(item => item.Value));
                listBoxForm.ShowDialog();
            }
        }
    }
}