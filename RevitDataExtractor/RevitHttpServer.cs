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

        public void SetUIApplication(UIApplication uiApp)
        {
            _uiApp = uiApp;
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            listener.BeginGetContext(OnRequestReceived, listener);
        }

        public void Stop()
        {
            if (listener != null)
            {
                listener.Stop();
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