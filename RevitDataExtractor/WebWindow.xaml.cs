using Autodesk.Revit.UI;
using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.Diagnostics;
using WallDataPlugin;

/// <summary>
/// For webwindow opening.
/// </summary>

namespace RevitDataExtractor
{
    public partial class WebWindow : Window
    {
        public UIApplication uiApp;
        private string url;

        public WebWindow(UIApplication app, string url)
        {
            InitializeComponent();
            uiApp = app;
            this.url = url;
            webView.Source = new Uri(url);
        }

        private void OnWebViewInteraction(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            WvReceiveAction result = null;
            try
            {
                result = JsonConvert.DeserializeObject<WvReceiveAction>(e.WebMessageAsJson);
                Debug.WriteLine(result?.action);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing message: {ex.Message}");
            }

            if (result == null) return;

            var exporter = new WallDataExporter();

            switch (result.action)
            {
                case "GetVersion":
                    App.Raise(() => exporter.GetRevitVersion(uiApp.Application));
                    break;

                case "GetWallData":
                    App.Raise(() => exporter.CollectWallData(uiApp.ActiveUIDocument.Document));
                    break;

                default:
                    Debug.WriteLine("Unhandled action: " + result.action);
                    break;
            }
        }

        internal class WvReceiveAction
        {
            public string action { get; set; }
            public object payload { get; set; }
        }
    }
}
