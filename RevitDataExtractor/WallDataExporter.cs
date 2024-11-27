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

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    public class WallDataExporter : IExternalCommand
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Autodesk.Revit.ApplicationServices.Application _revitApp;
        private static Document _revitDoc;
        private static string _serverUrl = "http://127.0.0.1:8080"; // Define the server URL

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

        // Function to export wall data to CSV
        public void ExportWallDataToCsv(Document doc, string filePath)
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
        public string CollectWallData(Document doc)
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
