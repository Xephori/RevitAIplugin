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
            CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(userDataFolder: DEFAULT_FOLDER);
            await web_view.EnsureCoreWebView2Async(env);

            web_view.CoreWebView2.AddWebResourceRequestedFilter(FAKE_URL + "*",
                CoreWebView2WebResourceContext.All
                );
            web_view.CoreWebView2.WebResourceRequested += delegate (object sender,
                CoreWebView2WebResourceRequestedEventArgs args)
            {
                string assets_file_path = Utilities.AssemblyDirectory
               + "/ui/" + args.Request.Uri.Substring((FAKE_URL + "*").Length - 1);

                Debug.WriteLine(Utilities.AssemblyDirectory);
                Debug.WriteLine(assets_file_path);
                Debug.WriteLine(args.Request.Uri);

                try
                {
                    FileStream fs = File.OpenRead(assets_file_path);
                    ManagedStream ms = new ManagedStream(fs);
                    string headers = "";
                    if (assets_file_path.EndsWith(".html"))
                    {
                        headers = "Content-Type: text/html";
                    }
                    else if (assets_file_path.EndsWith(".jpg"))
                    {
                        headers = "Content-Type: image/jpeg";
                    }
                    else if (assets_file_path.EndsWith(".png"))
                    {
                        headers = "Content-Type: image/png";
                    }
                    else if (assets_file_path.EndsWith(".css"))
                    {
                        headers = "Content-Type: text/css";
                    }
                    else if (assets_file_path.EndsWith(".js"))
                    {
                        headers = "Content-Type: application/javascript";
                    }
                    else if (assets_file_path.EndsWith(".json")
                            || assets_file_path.EndsWith(".map"))
                    {
                        headers = "Content-Type: application/json";
                    }
                    args.Response = web_view.CoreWebView2.Environment.CreateWebResourceResponse(
                                                            ms, 200, "OK", headers);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to get.");
                    Debug.WriteLine(ex.Message);
                    args.Response = web_view.CoreWebView2.Environment.CreateWebResourceResponse(
                                                        null, 404, "Not found", "");
                }
            };
            // FOR embedded web files: .html needed because for some reason can't read from root url.
            // web_view.CoreWebView2.Navigate(FAKE_URL+"index.html");
            // For development or connection to a remote server, just navigate to the website
            web_view.CoreWebView2.Navigate("http://localhost:5173");
        }
    }

}
