using System;
using Autodesk.Revit.UI;
using System.Diagnostics;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Windows.Forms; // Add this namespace

/// <summary>
/// Launches Web UI
/// Author: Bob Lee
/// </summary>

namespace RevitWebApp
{
    public class LaunchWeb
    {
        private WebView2 web_view;
        public Action CloseAction { get; set; }
        private const string DEFAULT_FOLDER = "C:/Temp";
        private const string FAKE_URL = "fake_url";

        internal LaunchWeb(UIApplication a, WebView2 web_view)
        {
            this.web_view = web_view;
            // added to start the server from revit, otherwise, start it manually from terminal
            // note to remove this if the server is going to be hosted online
            StartHttpServer();

            this.LoadContent();
        }

        //definition to start the http server from revit
        //to remove this part if the server is going to be hosted online
        private void StartHttpServer()
        {
            ProcessStartInfo npmStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c npm start",
                // Arguments = "/c http-server -p 5173",
                WorkingDirectory = @"C:\Users\trust\OneDrive - Singapore University of Technology and Design\Internship\RevitAIplugin\localhost", // Update this path
                CreateNoWindow = true,
                UseShellExecute = false
            };

            ProcessStartInfo pythonStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c \"cd /d C:\\Users\\trust\\OneDrive - Singapore University of Technology and Design\\Internship\\RevitAIplugin\\python && venv\\Scripts\\activate && python app.py\"",
                WorkingDirectory = @"C:\Users\trust\OneDrive - Singapore University of Technology and Design\Internship\RevitAIplugin\python", // Update this path
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                Process npmProcess = Process.Start(npmStartInfo);
                if (npmProcess == null)
                {
                    throw new InvalidOperationException("Failed to start npm server.");
                }

                Process pythonProcess = Process.Start(pythonStartInfo);
                if (pythonProcess == null)
                {
                    throw new InvalidOperationException("Failed to start Python server.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to start servers.");
                Debug.WriteLine(ex.Message);
            }
        }

            private async void LoadContent()
        {
            var startupForm = new StartupForm();
            if (startupForm.ShowDialog() == DialogResult.OK)
            {
                if (startupForm.UseLocalhost)
                {
                    StartHttpServer();
                    await web_view.EnsureCoreWebView2Async(null); // Corrected variable name
                    web_view.Source = new Uri("http://localhost:5173"); // Corrected variable name
                }
                else
                {
                    await web_view.EnsureCoreWebView2Async(null); // Corrected variable name
                    web_view.Source = new Uri(FAKE_URL); // Corrected variable name
                }
            }
        }
    }
}
