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
    public partial class AudioForm : Form
    {
        public AudioForm()
        {
            InitializeComponent();
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

        private void button2_Click(object sender, EventArgs e)
        {
            Database database = MdiParent1.Database;
            Dictionary<int, Contact> dictionary =
                database.Load(new Contact())
                    .ToDictionary(contact => Database.ConvertTo<int>(((Contact) contact).Id),
                        contact => (Contact) contact);
            using (var spectrumBuilder = new SpectrumBuilder(MdiParent1.SpectrumLength, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        spectrumBuilder.Add(waveFileReader);
                }
                double[] data = spectrumBuilder.GetData().ToArray();
                List<ClientCorrelation> list =
                    MdiParent1.ContactClassifier.Select(character => new ClientCorrelation
                    {
                        Id = character.Key,
                        Value = new CorrelationBuilder(data, character.Value).GetValue()
                    }).ToList();
                list.Sort();
                var listBoxForm = new ListBoxForm(list.Select(item => dictionary[item.Id]).Cast<object>(),
                    list.Select(item => item.Value));
                listBoxForm.ShowDialog();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        waveBuilder.Add(waveFileReader);
                }
                double[] data = waveBuilder.GetData().ToArray();
                var soundCorrelations = new List<SoundCorrelation>();
                foreach (var sound in MdiParent1.SoundsClassifier)
                {
                    string word = sound.Key;
                    var fftw = new fftw_complexarray(data.Zip(sound.Value, (x, y) => (Complex) (x*y)).ToArray());
                    List<double> array = fftw.GetData_Real().ToList();
                    array.Sort();
                    if (array.Count > MdiParent1.SinglePhonemeCount)
                        array.RemoveRange(0, array.Count - MdiParent1.SinglePhonemeCount);
                    soundCorrelations.AddRange(
                        array.Select(value => new SoundCorrelation {Phoneme = word, Value = value}));
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

        private void button4_Click(object sender, EventArgs e)
        {
            Database database = MdiParent1.Database;
            Dictionary<int, Contact> dictionary =
                database.Load(new Contact())
                    .ToDictionary(contact => Database.ConvertTo<int>(((Contact) contact).Id),
                        contact => (Contact) contact);
            using (var spectrumBuilder = new SpectrumBuilder(MdiParent1.SpectrumLength, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        spectrumBuilder.Add(waveFileReader);
                }
                double[] data = spectrumBuilder.GetData().ToArray();
                List<ClientCorrelation> clientCorrelations =
                    MdiParent1.ContactClassifier.Select(character => new ClientCorrelation
                    {
                        Id = character.Key,
                        Value = new CorrelationBuilder(data, character.Value).GetValue()
                    }).ToList();
                clientCorrelations.Sort();
                int id = clientCorrelations[0].Id;
                List<string> password;
                List<string> password1;
                using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
                {
                    foreach (Record audioFile in database.Load(new AudioFile {ContactId = id}))
                        using (
                            var waveFileReader =
                                new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder,
                                    Database.ConvertTo<string>(((AudioFile) audioFile).FileName))))
                            waveBuilder.Add(waveFileReader);
                    double[] doubles = waveBuilder.GetData().ToArray();
                    var soundCorrelations = new List<SoundCorrelation>();
                    foreach (var sound in MdiParent1.SoundsClassifier)
                    {
                        string word = sound.Key;
                        var fftw = new fftw_complexarray(doubles.Zip(sound.Value, (x, y) => (Complex) (x*y)).ToArray());
                        List<double> array = fftw.GetData_Real().ToList();
                        array.Sort();
                        if (array.Count > MdiParent1.SinglePhonemeCount)
                            array.RemoveRange(0, array.Count - MdiParent1.SinglePhonemeCount);
                        soundCorrelations.AddRange(
                            array.Select(value => new SoundCorrelation {Phoneme = word, Value = value}));
                        soundCorrelations.Sort();
                        if (soundCorrelations.Count > MdiParent1.TotalPhonemeCount)
                            soundCorrelations.RemoveRange(MdiParent1.TotalPhonemeCount,
                                soundCorrelations.Count - MdiParent1.TotalPhonemeCount);
                    }
                    password = soundCorrelations.Select(soundCorrelation => soundCorrelation.Phoneme).ToList();
                    password.Sort();
                }
                using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
                {
                    foreach (string file in recordingPanel1.Files)
                        using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                            waveBuilder.Add(waveFileReader);
                    double[] doubles = waveBuilder.GetData().ToArray();
                    var soundCorrelations = new List<SoundCorrelation>();
                    foreach (var sound in MdiParent1.SoundsClassifier)
                    {
                        string word = sound.Key;
                        var fftw = new fftw_complexarray(doubles.Zip(sound.Value, (x, y) => (Complex) (x*y)).ToArray());
                        List<double> array = fftw.GetData_Real().ToList();
                        array.Sort();
                        if (array.Count > MdiParent1.SinglePhonemeCount)
                            array.RemoveRange(0, array.Count - MdiParent1.SinglePhonemeCount);
                        soundCorrelations.AddRange(
                            array.Select(value => new SoundCorrelation {Phoneme = word, Value = value}));
                        soundCorrelations.Sort();
                        if (soundCorrelations.Count > MdiParent1.TotalPhonemeCount)
                            soundCorrelations.RemoveRange(MdiParent1.TotalPhonemeCount,
                                soundCorrelations.Count - MdiParent1.TotalPhonemeCount);
                    }
                    password1 = soundCorrelations.Select(soundCorrelation => soundCorrelation.Phoneme).ToList();
                    password1.Sort();
                }
                var contact = (Contact)database.Load(new Contact() { Id = id }).First();
                var accessGrantedForm = new AccessGrantedForm(password.SequenceEqual(password1))
                {
                    Id = Database.ConvertTo<int>(contact.Id),
                    FirstName = Database.ConvertTo<string>(contact.FirstName),
                    LastName = Database.ConvertTo<string>(contact.LastName),
                    Phone = Database.ConvertTo<string>(contact.Phone),
                    Email = Database.ConvertTo<string>(contact.Email),
                };
                accessGrantedForm.ShowDialog();
            }
        }

        private class ClientCorrelation : IComparable<ClientCorrelation>
        {
            public int Id { get; set; }
            public double Value { get; set; }

            public int CompareTo(ClientCorrelation obj)
            {
                double value = obj.Value;
                if (Value < value) return 1;
                if (Value > value) return -1;
                return 0;
            }
        }

        public class SoundCorrelation : IComparable<SoundCorrelation>
        {
            public string Phoneme { get; set; }
            public double Value { get; set; }

            public int CompareTo(SoundCorrelation obj)
            {
                double value = obj.Value;
                if (Value < value) return 1;
                if (Value > value) return -1;
                return 0;
            }
        }
    }
}