using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace vocoder.ClassLibrary
{
    [DefaultBindingProperty("Files")]
    public partial class RecordingPanel : UserControl
    {
        public delegate void AudioFileHandler(object obj, string file);

        private string _outputFilename;
        private IWaveIn _waveIn;
        private WaveFileWriter _writer;

        public RecordingPanel()
        {
            InitializeComponent();
            Disposed += OnRecordingPanelDisposed;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                LoadWasapiDevicesCombo();
            }
            else
            {
                radioButtonWasapi.Enabled = false;
                comboWasapiDevices.Enabled = false;
                radioButtonWasapiLoopback.Enabled = false;
            }

            // close the device if we change option only
            radioButtonWasapi.CheckedChanged += (s, a) => Cleanup();
            radioButtonWaveIn.CheckedChanged += (s, a) => Cleanup();
            radioButtonWaveInEvent.CheckedChanged += (s, a) => Cleanup();
            radioButtonWasapiLoopback.CheckedChanged += (s, a) => Cleanup();
        }

        public static string AudioFolder { get; set; }

        public ListBox.ObjectCollection Files
        {
            get { return listBoxRecordings.Items; }
            set
            {
                listBoxRecordings.Items.Clear();
                if (value == null) return;
                foreach (object item in value)
                    listBoxRecordings.Items.Add(item);
            }
        }

        public event AudioFileHandler OnAddAudioFile;
        public event AudioFileHandler OnDeleteAudioFile;

        private void OnRecordingPanelDisposed(object sender, EventArgs e)
        {
            Cleanup();
        }

        private void LoadWasapiDevicesCombo()
        {
            var deviceEnum = new MMDeviceEnumerator();
            List<MMDevice> devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();

            comboWasapiDevices.DataSource = devices;
            comboWasapiDevices.DisplayMember = "FriendlyName";
        }

        private void OnButtonStartRecordingClick(object sender, EventArgs e)
        {
            if (radioButtonWaveIn.Checked)
                Cleanup(); // WaveIn is still unreliable in some circumstances to being reused

            if (_waveIn == null)
            {
                CreateWaveInDevice();
            }

            _outputFilename = String.Format("NAudioDemo {0:yyy-MM-dd HH-mm-ss}.wav", DateTime.Now);
            _writer = new WaveFileWriter(Path.Combine(AudioFolder, _outputFilename), _waveIn.WaveFormat);
            _waveIn.StartRecording();
            SetControlStates(true);
        }

        private void CreateWaveInDevice()
        {
            if (radioButtonWaveIn.Checked)
            {
                _waveIn = new WaveIn {WaveFormat = new WaveFormat(8000, 1)};
            }
            else if (radioButtonWaveInEvent.Checked)
            {
                _waveIn = new WaveInEvent {WaveFormat = new WaveFormat(8000, 1)};
            }
            else if (radioButtonWasapi.Checked)
            {
                // can't set WaveFormat as WASAPI doesn't support SRC
                var device = (MMDevice) comboWasapiDevices.SelectedItem;
                _waveIn = new WasapiCapture(device);
            }
            else
            {
                // can't set WaveFormat as WASAPI doesn't support SRC
                _waveIn = new WasapiLoopbackCapture();
            }
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<StoppedEventArgs>(OnRecordingStopped), sender, e);
            }
            else
            {
                FinalizeWaveFile();
                progressBar1.Value = 0;
                if (e.Exception != null)
                {
                    MessageBox.Show(String.Format("A problem was encountered during recording {0}",
                        e.Exception.Message));
                }
                int newItemIndex = listBoxRecordings.Items.Add(_outputFilename);
                listBoxRecordings.SelectedIndex = newItemIndex;
                SetControlStates(false);
            }
        }

        private void Cleanup()
        {
            if (_waveIn != null)
            {
                _waveIn.Dispose();
                _waveIn = null;
            }
            FinalizeWaveFile();
        }

        private void FinalizeWaveFile()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (InvokeRequired)
            {
                //Debug.WriteLine("Data Available");
                BeginInvoke(new EventHandler<WaveInEventArgs>(OnDataAvailable), sender, e);
            }
            else
            {
                //Debug.WriteLine("Flushing Data Available");
                _writer.Write(e.Buffer, 0, e.BytesRecorded);
                var secondsRecorded = (int) (_writer.Length/_writer.WaveFormat.AverageBytesPerSecond);
                if (secondsRecorded >= 30)
                {
                    StopRecording();
                }
                else
                {
                    progressBar1.Value = secondsRecorded;
                }
            }
        }

        private void StopRecording()
        {
            Debug.WriteLine("StopRecording");
            if (_waveIn != null) _waveIn.StopRecording();
            if (OnAddAudioFile != null) OnAddAudioFile(this, _outputFilename);
        }

        private void OnButtonStopRecordingClick(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void OnButtonPlayClick(object sender, EventArgs e)
        {
            if (listBoxRecordings.SelectedItem != null)
            {
                Process.Start(Path.Combine(AudioFolder, (string) listBoxRecordings.SelectedItem));
            }
        }

        private void SetControlStates(bool isRecording)
        {
            groupBoxRecordingApi.Enabled = !isRecording;
            buttonStartRecording.Enabled = !isRecording;
            buttonStopRecording.Enabled = isRecording;
        }

        private void OnButtonDeleteClick(object sender, EventArgs e)
        {
            if (listBoxRecordings.SelectedItem != null)
            {
                try
                {
                    var file = (string) listBoxRecordings.SelectedItem;
                    File.Delete(Path.Combine(AudioFolder, file));
                    listBoxRecordings.Items.Remove(file);
                    if (listBoxRecordings.Items.Count > 0) listBoxRecordings.SelectedIndex = 0;
                    if (OnDeleteAudioFile != null) OnDeleteAudioFile(this, file);
                }
                catch (Exception)
                {
                    MessageBox.Show(@"Could not delete recording");
                }
            }
        }

        private void OnOpenFolderClick(object sender, EventArgs e)
        {
            Process.Start(AudioFolder);
        }
    }
}