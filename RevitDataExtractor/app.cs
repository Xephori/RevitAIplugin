#region Namespaces
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
#endregion

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    class App : IExternalApplication
    {
        public static RevitHttpServer httpServer { get; private set; }
        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                string ribbonPanelName = "AI Plugin";
                RibbonPanel ribbonPanel = a.CreateRibbonPanel("Add-Ins", ribbonPanelName);

                string thisAssembly = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData(
                    "RevitAIPlugin",
                    "RevitAIPlugin",
                    thisAssembly,
                    "RevitDataExtractor.WebCommand"
                );

                ribbonPanel.AddItem(buttonData);

                httpServer = new RevitHttpServer();
                httpServer.Start();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Log the error to a file
                System.IO.File.WriteAllText(@"C:\temp\RevitPluginError.log", ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            if (httpServer != null)
            {
                httpServer.Stop();
            }

            return Result.Succeeded;
        }
    }
}