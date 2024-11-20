using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using WallDataPlugin;
using System.Threading.Tasks;

namespace RevitDataExtractor
{
    // A utility class for shared functionality
    public static class RevitUtilities
    {
        // Function to retrieve the Revit version
        public static string GetRevitVersion(Autodesk.Revit.ApplicationServices.Application app)
        {
            return app.VersionNumber;
        }

        // Function to export wall data to CSV
        public static void ExportWallDataToCsv(Document doc, string filePath)
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
                WallKind kind = wallType.Kind;
                csvLines.Add($"{name},{width:F2},{kind}");
            }

            // Write CSV to file
            File.WriteAllLines(filePath, csvLines);
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    public class WallDataExporter : IExternalCommand
    {
        // Main entry point of the plugin
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
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

        public Result OnStartup(UIApplication application)
        {
            Task.Run(() =>
            {
                var httpServer = new HttpServer(application);
                httpServer.Start();
            });
            return Result.Succeeded;
        }

    }

    public class HttpServer
    {
        private HttpListener listener;
        private UIApplication uiApp; // Reference to Revit application

        public HttpServer(UIApplication application)
        {
            uiApp = application;
        }

        public void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8080/");
            listener.Start();
            Console.WriteLine("HTTP Server is running...");

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                string responseString = HandleRequest(request);
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;

                using (System.IO.Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private string HandleRequest(HttpListenerRequest request)
        {
            string route = request.Url.AbsolutePath.ToLower();

            if (route == "/get-revit-version")
            {
                return RevitUtilities.GetRevitVersion(uiApp.Application);
            }
            else if (route == "/export-wall-data")
            {
                string filePath = request.QueryString["filepath"];
                if (string.IsNullOrEmpty(filePath))
                {
                    return "File path is required for exporting wall data.";
                }

                RevitUtilities.ExportWallDataToCsv(uiApp.ActiveUIDocument.Document, filePath);
                return $"Wall data exported to {filePath}";
            }
            else
            {
                return "Invalid route.";
            }
        }
    }
}
