using System;
using Autodesk.Revit.UI;
using System.Diagnostics;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.IO;

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
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c npm start",
                // Arguments = "/c http-server -p 5173",
                WorkingDirectory = @"C:\Users\trust\OneDrive - Singapore University of Technology and Design\Internship\RevitAIplugin\localhost", // Update this path
                CreateNoWindow = true,
                UseShellExecute = false
            };

            try
            {
                Process process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start HTTP server.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to start HTTP server.");
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
                    await webView2.EnsureCoreWebView2Async(null);
                    webView2.Source = new Uri("http://localhost:5173");
                }
                else
                {
                    await webView2.EnsureCoreWebView2Async(null);
                    webView2.Source = new Uri("https://placeholder-url.com");
                }
            }
        }
    }

}
