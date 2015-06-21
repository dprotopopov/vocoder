using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using vocoder.ClassLibrary;
using vocoder.DatabaseLibrary;

namespace vocoder
{
    public partial class MdiParent1 : Form
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int PhonemeLength = 2;
        public static int SpectrumLength = 1024;
        public static int SinglePhonemeCount = 1;
        public static int TotalPhonemeCount = 7;
        public static double Duration = 1.5;
        public static int Frequency = 8000;
        public static string AudioFolder = "NAudio";
        public static string DatabaseFilename = "database.sqlite";
        public static Database Database = new Database();

        public static readonly Dictionary<int, double[]> ContactClassifier = new Dictionary<int, double[]>();
        public static readonly Dictionary<string, Complex[]> SoundsClassifier = new Dictionary<string, Complex[]>();
        private int _childFormNumber;

        public MdiParent1()
        {
            InitializeComponent();
            RecordingPanel.AudioFolder = AudioFolder;
            Directory.CreateDirectory(AudioFolder);
            Database.DatabaseFilename = DatabaseFilename;
            Database.CreateDatabaseIfNotExists();
            Database.Connect();
            RebuildContactClassifier();
            RebuildSampleClassifier();
        }

        private static void RebuildSampleClassifier()
        {
            SoundsClassifier.Clear();
            int alphabetLength = Alphabet.Length;
            var total = (long) Math.Pow(alphabetLength, PhonemeLength);
            for (long i = 0; i < total; i++)
            {
                var sb = new StringBuilder();
                int index = 0;
                for (long j = i; index++ < PhonemeLength; j /= alphabetLength)
                    sb.Append(Alphabet.Substring((int) (j%alphabetLength), 1));
                string word = sb.ToString();
                Debug.WriteLine(word);
                using (var builder = new SoundsBuilder(word, Duration, Frequency))
                    SoundsClassifier.Add(word, builder.GetData_Complex());
            }
        }

        private static void RebuildContactClassifier()
        {
            ContactClassifier.Clear();
            foreach (
                int id in
                    Database.Load(new Contact()).Select(contact => Database.ConvertTo<int>(((Contact) contact).Id)))
            {
                using (var spectrumBuilder = new SpectrumBuilder(SpectrumLength, Frequency))
                {
                    foreach (
                        string file in
                            Database.Load(new AudioFile {ContactId = id})
                                .Select(
                                    audioFile =>
                                        Path.Combine(AudioFolder,
                                            Database.ConvertTo<string>(((AudioFile) audioFile).FileName))))
                        spectrumBuilder.Add(new WaveFileReader(file));
                    ContactClassifier.Add(id, spectrumBuilder.GetData());
                }
            }
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            var childForm = new AudioForm
            {
                MdiParent = this,
                Text = @"Окно " + _childFormNumber++
            };
            childForm.Show();
        }

        private void OpenFile(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Filter = @"Звуковые файлы (*.wav)|*.wav|Все файлы (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;
            string fileName = openFileDialog.FileName;
            var childForm = new AudioForm
            {
                MdiParent = this,
                Text = @"Окно " + _childFormNumber++
            };
            childForm.Show();
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                Filter = @"Звуковые файлы (*.wav)|*.wav|Все файлы (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void CascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void ArrangeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var about = new AboutBox1();
            about.ShowDialog();
        }

        private void databaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var contactForm = new DatabaseForm();
            contactForm.ShowDialog();
            RebuildContactClassifier();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var optionsForm = new OptionsForm
            {
                SpectrumLength = SpectrumLength,
                Frequency = Frequency,
                AudioFolder = AudioFolder,
                Duration = Duration
            };
            optionsForm.ShowDialog();
            SpectrumLength = optionsForm.SpectrumLength;
            Frequency = optionsForm.Frequency;
            AudioFolder = optionsForm.AudioFolder;
            Duration = optionsForm.Duration;
            RecordingPanel.AudioFolder = AudioFolder;
            RebuildContactClassifier();
            RebuildSampleClassifier();
        }
    }
}