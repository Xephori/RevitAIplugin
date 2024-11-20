﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WallDataPlugin
{
    public partial class UserForm : Form
    {
        public UserForm()
        {
            InitializeComponent();
        }

        private void btnLaunchWebUI_Click(object sender, EventArgs e)
        {
            // Check if 'Launch Localhost' checkbox is checked
            if (chkLaunchLocalhost.Checked)
            {
                // Launch a localhost server
                StartLocalhostServer();
            }
            else
            {
                // Open a deployed web link
                string webLink = txtWebLink.Text;
                if (!string.IsNullOrEmpty(webLink))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = webLink,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Please enter a valid web link.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StartLocalhostServer()
        {
            try
            {
                // Define the Python script to run the localhost server
                string pythonScript = @"C:\Users\trust\OneDrive - Singapore University of Technology and Design\Internship\RevitAIplugin\python\streamlit\streamlit_app.py";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = pythonScript,
                    UseShellExecute = true,
                    CreateNoWindow = false
                });

                // Open the localhost URL in the default browser
                Process.Start(new ProcessStartInfo
                {
                    FileName = "http://localhost:8501", // Adjust based on your Streamlit app port
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start localhost server. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

