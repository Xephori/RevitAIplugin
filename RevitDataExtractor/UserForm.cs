using Autodesk.Revit.UI;
using Microsoft.Web.WebView2.WinForms;
using RevitDataExtractor;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WallDataPlugin
{
    public partial class UserForm : Form 
    { 
        private UIApplication uiApp;
        private WebView2 webView2Control;
        private NgrokHelper ngrokHelper;
        public UserForm(UIApplication app, WebView2 webView)
        {
            InitializeComponent();
            uiApp = app;
            webView2Control = webView;
            ngrokHelper = new NgrokHelper();
        }

        public string WebLink
        {
            get { return txtWebLink.Text; }
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

                    RevitHttpServer httpServer = new RevitHttpServer();
                    httpServer.SetUIApplication(uiApp);
                    httpServer.Start();

                    LaunchWeb launchWeb = new LaunchWeb(uiApp, webView2Control);
                    launchWeb.LoadContent();
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

        private async Task<string> StartNgrokProcess()
        {
            try
            {
                int port = 8080; // Adjust based on your needs
                string ngrokUrl = await ngrokHelper.StartNgrok(port);
                return ngrokUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start ngrok process. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}

