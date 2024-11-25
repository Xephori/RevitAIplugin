#region Namespaces
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
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
            return Result.Succeeded;
        }
    }
}