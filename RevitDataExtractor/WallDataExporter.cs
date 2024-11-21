//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Autodesk.Revit.UI;
//using Autodesk.Revit.DB;
//using System.Windows.Forms;
//using WallDataPlugin;
//using Autodesk.Revit.Attributes;
//namespace RevitDataExtractor
//{
//    [Transaction(TransactionMode.Manual)]
//    public class WallDataExporter : IExternalCommand
//    {
//        // Main entry point of the plugin
//        public Result Execute(
//    ExternalCommandData commandData,
//    ref string message,
//    ElementSet elements)
//        {
//            try
//            {
//                // Show the user form
//                UserForm form = new UserForm();
//                form.ShowDialog();

//                return Result.Succeeded;
//            }
//            catch (Exception ex)
//            {
//                message = ex.Message;
//                return Result.Failed;
//            }
//        }

//        // Function to retrieve the Revit version
//        private string GetRevitVersion(Autodesk.Revit.ApplicationServices.Application app)
//        {
//            return app.VersionNumber;
//        }

//        // Function to export wall data to CSV
//        private void ExportWallDataToCsv(Document doc, string filePath)
//        {
//            // Collect all wall elements in the document
//            FilteredElementCollector collector = new FilteredElementCollector(doc);
//            ICollection<Element> wallTypes = collector.OfClass(typeof(WallType)).ToElements();

//            // Prepare CSV data
//            List<string> csvLines = new List<string> { "WallType Name,Width (m),Function" };

//            foreach (WallType wallType in wallTypes)
//            {
//                string name = wallType.Name;
//                double width = wallType.Width; // Width is stored in feet
//                width = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Meters);
//                WallFunction function = (WallFunction)(int)wallType.Kind;

//                csvLines.Add($"{name},{width:F2},{function}");
//            }

//            // Write CSV to file
//            File.WriteAllLines(filePath, csvLines);
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using WallDataPlugin;
using Autodesk.Revit.Attributes;
using Newtonsoft.Json;

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    public class WallDataExporter : IExternalCommand
    {
        private static HttpListener _listener;
        private static Autodesk.Revit.ApplicationServices.Application _revitApp;
        private static Document _revitDoc;

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                _revitApp = commandData.Application.Application;
                _revitDoc = commandData.Application.ActiveUIDocument.Document;

                // Start the HTTP server
                StartHttpServer();

                // Show the user form
                UserForm form = new UserForm();
                form.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void StartHttpServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
            _listener.BeginGetContext(new AsyncCallback(OnRequestReceived), null);
            Task.Run(() => Console.WriteLine("HTTP server started on http://localhost:8080/"));
        }

        private async void OnRequestReceived(IAsyncResult result)
        {
            if (_listener == null || !_listener.IsListening)
                return;

            HttpListenerContext context = _listener.EndGetContext(result);
            _listener.BeginGetContext(new AsyncCallback(OnRequestReceived), null);

            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.HttpMethod == "GET")
            {
                if (request.Url.AbsolutePath == "/get-revit-version")
                {
                    string revitVersion = GetRevitVersion(_revitApp);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    using (StreamWriter writer = new StreamWriter(response.OutputStream))
                    {
                        writer.Write(revitVersion);
                    }
                }
                else if (request.Url.AbsolutePath == "/export-wall-data")
                {
                    string filePath = request.QueryString["filepath"];
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        ExportWallDataToCsv(_revitDoc, filePath);
                        response.StatusCode = (int)HttpStatusCode.OK;
                        using (StreamWriter writer = new StreamWriter(response.OutputStream))
                        {
                            writer.Write("Wall data exported successfully.");
                        }
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        using (StreamWriter writer = new StreamWriter(response.OutputStream))
                        {
                            writer.Write("File path is required.");
                        }
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else if (request.HttpMethod == "POST")
            {
                using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestData = await reader.ReadToEndAsync();
                    await ForwardDataToApiAsync(requestData);
                }

                response.StatusCode = (int)HttpStatusCode.OK;
                using (StreamWriter writer = new StreamWriter(response.OutputStream))
                {
                    writer.Write("Data received and forwarded successfully.");
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }

            response.Close();
        }

        private async Task ForwardDataToApiAsync(string jsonData)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8501/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("api/revitdata", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to forward data to API: " + response.ReasonPhrase);
                }
            }
        }

        // Function to retrieve the Revit version
        private string GetRevitVersion(Autodesk.Revit.ApplicationServices.Application app)
        {
            return app.VersionNumber;
        }

        // Function to export wall data to CSV
        private void ExportWallDataToCsv(Document doc, string filePath)
        {
            // Collect all wall elements in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallTypes = collector.OfClass(typeof(WallType)).ToElements();

            // Prepare CSV data
            List<string> csvLines = new List<string> { "WallType Name,Width (m),Function" };

            foreach (WallType wallType in wallTypes)
            {
                string name = wallType.Name;
                double width = wallType.Width; // Width is stored in feet
                width = UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Meters);
                WallFunction function = (WallFunction)(int)wallType.Kind;

                csvLines.Add($"{name},{width:F2},{function}");
            }

            // Write CSV to file
            File.WriteAllLines(filePath, csvLines);
        }

        // Function to collect wall data
        private string CollectWallData(Document doc)
        {
            // Collect all wall elements in the document
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallTypes = collector.OfClass(typeof(WallType)).ToElements();

            // Prepare data
            var wallDataList = new List<object>();

            foreach (WallType wallType in wallTypes)
            {
                var wallData = new
                {
                    Name = wallType.Name,
                    Width = UnitUtils.ConvertFromInternalUnits(wallType.Width, UnitTypeId.Meters),
                    Function = (WallFunction)(int)wallType.Kind
                };
                wallDataList.Add(wallData);
            }

            // Convert to JSON
            return JsonConvert.SerializeObject(wallDataList);
        }
    }
}
