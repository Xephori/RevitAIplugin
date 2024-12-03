using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Newtonsoft.Json;

/// <summary>
/// For http requests and server running.
/// </summary>

namespace RevitDataExtractor
{
    public class RevitHttpServer
    {
        private HttpListener listener;
        private UIApplication _uiApp;
        private bool isRunning = false;
        private NgrokHelper ngrokHelper;
        private int port;

        public void SetUIApplication(UIApplication app)
        {
            if (_uiApp == null)
            {
                _uiApp = app;
            }
        }

        public async Task Start()
        {
            if (isRunning)
                return;

            port = 8080;
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:{port}/");
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine($"HTTP Server started on port {port}. Listening for POST requests...");

                // Start ngrok
                ngrokHelper = new NgrokHelper();
                string publicUrl = await ngrokHelper.StartNgrok(port);
                if (!string.IsNullOrEmpty(publicUrl))
                {
                    Console.WriteLine($"ngrok started. Public URL: {publicUrl}");
                }
                else
                {
                    Console.WriteLine("Failed to retrieve ngrok public URL.");
                }

                // Begin listening for requests
                Task.Run(() => ListenAsync());
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine($"Failed to start HTTP listener: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (listener != null && isRunning)
            {
                listener.Stop();
                listener.Close();
                isRunning = false;
                Console.WriteLine("HTTP Server stopped.");
            }

            ngrokHelper?.StopNgrok();
        }

        private async Task ListenAsync()
        {
            while (isRunning)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    Task.Run(() => HandleRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    // Listener was stopped, exit the loop
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving request: {ex.Message}");
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                // Add CORS headers to allow requests from specific origins
                response.Headers.Add("Access-Control-Allow-Origin", "https://revitaiplugin.streamlit.app/");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    return;
                }

                if (request.Url.AbsolutePath == "/getwalldata")
                {
                    //var wallData = GetWallData();
                    //string responseString = JsonConvert.SerializeObject(wallData);
                    //byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    //response.ContentLength64 = buffer.Length;
                    //response.ContentType = "application/json";
                    //await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                    var csvPath = ExportWallDataToCsv();
                    byte[] buffer = File.ReadAllBytes(csvPath);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/csv";
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else if (request.Url.AbsolutePath == "/getrevitversion")
                {
                    var revitVersion = GetRevitVersion();
                    string responseString = JsonConvert.SerializeObject(revitVersion);
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "application/json";
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    string responseString = "Invalid request. Supported endpoints: /getwalldata, /getrevitversion";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                //LogMessage($"Error handling request: {ex.Message}");
                string errorResponse = $"Error: {ex.Message}";
                byte[] buffer = Encoding.UTF8.GetBytes(errorResponse);
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                response.Close();
            }
        }
        private object GetRevitVersion()
        {
            if (_uiApp == null)
            {
                throw new InvalidOperationException("UIApplication is not set.");
            }

            WallDataExporter exporter = new WallDataExporter();
            string versionJson = exporter.GetRevitVersion(_uiApp.Application);
            return JsonConvert.DeserializeObject(versionJson);
        }

        private string ExportWallDataToCsv()
        {
            if (_uiApp == null)
            {
                throw new InvalidOperationException("UIApplication is not set.");
            }

            Document doc = _uiApp.ActiveUIDocument.Document; // Get the Document object from _uiApp
            WallDataExporter exporter = new WallDataExporter();
            string csvPath = exporter.ExportWallDataToCsv(doc);

            return csvPath;
        }
    }
}
