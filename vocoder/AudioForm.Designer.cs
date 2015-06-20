using vocoder.ClassLibrary;

namespace vocoder
{
    partial class AudioForm
    {
        public RecordingPanel recordingPanel1;
        public System.Windows.Forms.Button button1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.recordingPanel1 = new RecordingPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // recordingPanel1
            // 
            this.recordingPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.recordingPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.recordingPanel1.Location = new System.Drawing.Point(0, 0);
            this.recordingPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.recordingPanel1.Name = "recordingPanel1";
            this.recordingPanel1.Size = new System.Drawing.Size(1013, 494);
            this.recordingPanel1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(46, 530);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(324, 40);
            this.button1.TabIndex = 1;
            this.button1.Text = "Show Spectrum";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AudioForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1013, 611);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.recordingPanel1);
            this.Name = "AudioForm";
            this.Text = "AudioForm";
            this.ResumeLayout(false);

        }

        #endregion

    }
}