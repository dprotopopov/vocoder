﻿using System.Windows.Forms;

namespace vocoder.DatabaseLibrary
{
    public partial class ContactForm : Form
    {
        public ContactForm()
        {
            InitializeComponent();
        }

        public int Id
        {
            get { return (int) numericUpDown1.Value; }
            set { numericUpDown1.Value = value; }
        }

        public string FirstName
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public string LastName
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }

        public string Phone
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }

        public string Email
        {
            get { return textBox4.Text; }
            set { textBox4.Text = value; }
        }
    }
}