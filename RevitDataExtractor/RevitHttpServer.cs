using System;
using System.Net;
using System.Text;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace RevitDataExtractor
{
    public class RevitHttpServer
    {
        private HttpListener listener;
        private UIApplication _uiApp;
        private bool isRunning = false;

        public void SetUIApplication(UIApplication uiApp)
        {
            _uiApp = uiApp;
        }

        public void Start()
        {
            if (isRunning)
                return;

            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine("HTTP Server started. Listening for POST requests...");
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
        private void OnRequestReceived(IAsyncResult result)
        {
            HttpListener contextListener = (HttpListener)result.AsyncState;
            if (contextListener.IsListening)
            {
                HttpListenerContext context = contextListener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/get_wall_data")
                {
                    // Get the wall data from Revit
                    var wallData = GetWallData();
                    string responseString = JsonConvert.SerializeObject(wallData);

                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "application/json";
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.OutputStream.Close();
                contextListener.BeginGetContext(OnRequestReceived, contextListener);
            }

            if (request.HttpMethod == "POST" && request.ContentType == "text/csv")
            {
                try
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string csvData = await reader.ReadToEndAsync();
                        var records = ParseCsv(csvData);

                        // Process the records within Revit's context
                        _uiApp?.Invoke(() =>
                        {
                            // Example: Log the received data
                            foreach (var record in records)
                            {
                                TaskDialog.Show("Received Data", $"Column1: {record.Column1}, Column2: {record.Column2}");
                            }
                        });
                    }

                    string responseString = "CSV data received and processed successfully.";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing request: {ex.Message}");
                    string errorResponse = $"Error: {ex.Message}";
                    byte[] buffer = Encoding.UTF8.GetBytes(errorResponse);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            else
            {
                string invalidResponse = "Invalid request. Please send a POST request with 'text/csv' content type.";
                byte[] buffer = Encoding.UTF8.GetBytes(invalidResponse);
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            response.Close();
        }

        private object GetWallData()
        {
            if (_uiApp == null)
            {
                throw new InvalidOperationException("UIApplication is not set.");
            }

            // Use the CollectWallData method from WallDataExporter
            WallDataExporter exporter = new WallDataExporter();
            string wallDataJson = exporter.CollectWallData(_uiApp.ActiveUIDocument.Document);
            return JsonConvert.DeserializeObject(wallDataJson);
        }
    }
}