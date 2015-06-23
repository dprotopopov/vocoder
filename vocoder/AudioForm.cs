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
                double[] data = spectrumBuilder.GetData();
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
                Complex[] data = waveBuilder.GetData_Complex();
                var soundCorrelations = new List<SoundCorrelation>();
                foreach (var sound in MdiParent1.SoundsClassifier)
                {
                    string phoneme = sound.Key;
                    int count = data.Length;
                    Complex[] complexs = data.Zip(sound.Value, (x, y) => (x*y)).ToArray();
                    var input = new fftw_complexarray(complexs);
                    var output = new fftw_complexarray(count);
                    fftw_plan.dft_1d(count, input, output, fftw_direction.Backward, fftw_flags.Estimate).Execute();
                    List<double> list = output.GetData_Real().ToList();
                    list.Sort();
                    if (list.Count > MdiParent1.SinglePhonemeCount)
                        list.RemoveRange(0, list.Count - MdiParent1.SinglePhonemeCount);
                    soundCorrelations.AddRange(
                        list.Select(value => new SoundCorrelation {Phoneme = phoneme, Value = value}));
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
                    .ToDictionary(record => Database.ConvertTo<int>(((Contact) record).Id),
                        record => (Contact) record);
            int id1;
            int id2;
            using (var spectrumBuilder = new SpectrumBuilder(MdiParent1.SpectrumLength, MdiParent1.Frequency))
            {
                foreach (string file in recordingPanel1.Files)
                {
                    using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                        spectrumBuilder.Add(waveFileReader);
                }
                List<ClientCorrelation> clientCorrelations =
                    MdiParent1.ContactClassifier.Select(character => new ClientCorrelation
                    {
                        Id = character.Key,
                        Value = new CorrelationBuilder(spectrumBuilder.GetData(), character.Value).GetValue()
                    }).ToList();
                clientCorrelations.Sort();
                id1 = clientCorrelations[0].Id;
            }

            var list = new List<ClientCorrelation>();

            foreach (string file in recordingPanel1.Files)
            {
                using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                {
                    using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
                    {
                        waveBuilder.Add(waveFileReader);
                        Complex[] data = waveBuilder.GetData_Complex(true);
                        foreach (var audioFile in MdiParent1.AudioFileClassifier)
                        {
                            int count = data.Length;
                            var input = new fftw_complexarray(data.Zip(audioFile.Value, (x, y) => (x*y)).ToArray());
                            var output = new fftw_complexarray(count);
                            fftw_plan.dft_1d(count, input, output, fftw_direction.Forward, fftw_flags.Estimate)
                                .Execute();
                            double value = output.GetData_Complex().Select(x => x.Magnitude).Max();
                            list.Add(new ClientCorrelation
                            {
                                Id = audioFile.Key,
                                Value = value
                            });
                        }
                    }
                }
            }
            list.Sort();
            id2 = list.First().Id;
            var contact = (Contact) database.Load(new Contact {Id = id1}).First();
            var accessGrantedForm = new AccessGrantedForm(id1 == id2)
            {
                Id = Database.ConvertTo<int>(contact.Id),
                FirstName = Database.ConvertTo<string>(contact.FirstName),
                LastName = Database.ConvertTo<string>(contact.LastName),
                Phone = Database.ConvertTo<string>(contact.Phone),
                Email = Database.ConvertTo<string>(contact.Email),
            };
            accessGrantedForm.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Database database = MdiParent1.Database;
            var list = new List<ClientCorrelation>();
            Dictionary<int, Contact> dictionary =
                database.Load(new Contact())
                    .ToDictionary(contact => Database.ConvertTo<int>(((Contact) contact).Id),
                        contact => (Contact) contact);
            foreach (string file in recordingPanel1.Files)
            {
                using (var waveFileReader = new WaveFileReader(Path.Combine(RecordingPanel.AudioFolder, file)))
                {
                    using (var waveBuilder = new WaveBuilder(MdiParent1.Duration, MdiParent1.Frequency))
                    {
                        waveBuilder.Add(waveFileReader);
                        Complex[] data = waveBuilder.GetData_Complex(true);
                        foreach (var audioFile in MdiParent1.AudioFileClassifier)
                        {
                            int count = data.Length;
                            var input = new fftw_complexarray(data.Zip(audioFile.Value, (x, y) => (x*y)).ToArray());
                            var output = new fftw_complexarray(count);
                            fftw_plan.dft_1d(count, input, output, fftw_direction.Forward, fftw_flags.Estimate)
                                .Execute();
                            double value = output.GetData_Complex().Select(x => x.Magnitude).Max();
                            list.Add(new ClientCorrelation
                            {
                                Id = audioFile.Key,
                                Value = value
                            });
                        }
                    }
                }
            }
            list.Sort();
            var listBoxForm = new ListBoxForm(list.Select(item => dictionary[item.Id]).Cast<object>(),
                list.Select(item => item.Value));
            listBoxForm.ShowDialog();
        }

        public class ClientCorrelation : IComparable<ClientCorrelation>
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