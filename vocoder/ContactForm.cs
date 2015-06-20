using System;
using System.Windows.Forms;
using NAudio.Wave;
using vocoder.ClassLibrary;

namespace vocoder
{
    public partial class ContactForm : Form
    {
        public ContactForm()
        {
            InitializeComponent();
        }

        private void contactsBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            Validate();
            contactsBindingSource.EndEdit();
            tableAdapterManager.UpdateAll(database1DataSet);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "database1DataSet.Contacts". При необходимости она может быть перемещена или удалена.
            contactsTableAdapter.Fill(database1DataSet.Contacts);
        }

        private void contactsBindingNavigatorSaveItem_Click_1(object sender, EventArgs e)
        {
            Validate();
            contactsBindingSource.EndEdit();
            tableAdapterManager.UpdateAll(database1DataSet);
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