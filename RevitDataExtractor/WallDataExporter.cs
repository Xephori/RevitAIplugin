using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Forms;
using WallDataPlugin;
using Autodesk.Revit.Attributes;
using Newtonsoft.Json;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    public class WallDataExporter : IExternalCommand
    {
        private static ClientWebSocket _webSocket;
        private static Autodesk.Revit.ApplicationServices.Application _revitApp;
        private static Document _revitDoc;

        public class WebWindow : System.Windows.Window
        {
            private UIApplication _uiApp;

            public WebWindow(UIApplication uiApp)
            {
                _uiApp = uiApp;
                this.Title = "Web Window";
                this.Width = 800;
                this.Height = 600;

                // Add WebView2 or other UI components here
                // Example:
                //var webView = new Microsoft.Web.WebView2.Wpf.WebView2();
                //this.Content = webView;
                //webView.Source = new Uri("https://example.com");
            }
        }

        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                _revitApp = commandData.Application.Application;
                _revitDoc = commandData.Application.ActiveUIDocument.Document;

                // Start the WebSocket connection
                StartWebSocketConnection();

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

        private async void StartWebSocketConnection()
        {
            _webSocket = new ClientWebSocket();
            Uri serverUri = new Uri("wss://revitaiplugin.streamlit.app/ws");
            await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
            Task.Run(() => ReceiveMessages());
        }

        private async Task ReceiveMessages()
        {
            var buffer = new byte[1024 * 4];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // Handle the received message
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
            }
        }

        private async Task SendMessageAsync(string message)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
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
