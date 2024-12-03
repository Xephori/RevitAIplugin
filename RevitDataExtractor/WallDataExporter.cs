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
using Newtonsoft.Json;
using Autodesk.Revit.Attributes;
using WallDataPlugin;
using Microsoft.Web.WebView2.WinForms;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

/// <summary>
/// For main functions of the plugin.
/// </summary>

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    public class WallDataExporter : IExternalCommand
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Autodesk.Revit.ApplicationServices.Application _revitApp;
        private static Document _revitDoc;
        private static string _serverUrl = "http://localhost:8080"; // Define the server URL

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                _revitApp = commandData.Application.Application;
                _revitDoc = commandData.Application.ActiveUIDocument.Document;

                UIApplication uiApp = commandData.Application;
                WebView2 webView = new WebView2();
                // Show the user form
                UserForm form = new UserForm(uiApp, webView);
                form.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                var content = new StringContent(message, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(_serverUrl, content);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync();
                // Handle the response data as needed
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"Request error: {ex.Message}");
            }
        }

        private async Task ReceiveResponseAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(_serverUrl);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync();
                // Process the received data
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request exceptions
                Console.WriteLine($"Response error: {ex.Message}");
            }
        }

        private void StartHttpCommunication()
        {
            // Example of sending a message
            Task.Run(() => SendMessageAsync("Your message here"));

            // Example of receiving a response
            Task.Run(() => ReceiveResponseAsync());
        }

        // Function to retrieve the Revit version
        public string GetRevitVersion(Autodesk.Revit.ApplicationServices.Application app)
        {
            return app.VersionNumber;
        }

        // Function to collect wall data
        public List<WallData> CollectWallData(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> wallElements = collector.OfClass(typeof(Wall)).ToElements();

            List<WallData> wallDataList = new List<WallData>();

            foreach (Wall wall in wallElements)
            {
                var wallType = doc.GetElement(wall.GetTypeId()) as WallType;
                if (wallType != null)
                {
                    var param = wallType.LookupParameter("Type Comments"); // Example parameter, can be replaced with any parameter name
                    string paramValue = param != null ? param.AsString() : "N/A";

                    WallData wallData = new WallData
                    {
                        WallId = wall.Id.IntegerValue,
                        WallName = wall.Name,
                        WallType = wallType.Name,
                        TypeComments = paramValue,
                        Width = UnitUtils.ConvertFromInternalUnits(wallType.Width, UnitTypeId.Meters)
                    };
                    wallDataList.Add(wallData);
                }
            }

            return wallDataList;
        }

        public string ExportWallDataToCsv(Document doc)
        {
            List<WallData> wallDataList = CollectWallData(doc);

            string csvPath = Path.Combine("C:\\Users\\trust\\OneDrive - Singapore University of Technology and Design\\Internship\\RevitAIplugin\\python\\temp", "WallDataExport.csv");
            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(wallDataList); // Write all wall data records at once
            }

            return csvPath;
        }
    }
    public class WallData
    {
        public int WallId { get; set; }
        public string WallName { get; set; }
        public string WallType { get; set; }
        public string TypeComments { get; set; }
        public double Width { get; set; }
    }
}
