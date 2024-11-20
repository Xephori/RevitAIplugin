#region Namespaces
using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    class App : IExternalApplication
    {
        public static RevitEventHandler rvtHandler;
        public Result OnStartup(UIControlledApplication a)
        {
            rvtHandler = new RevitEventHandler();
            a.CreateRibbonTab("RevitDataExtractor");
            RibbonPanel ribbon = a.CreateRibbonPanel("RevitDataExtractor", "Open");

            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            //Stream imgStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("fire_rvt.images.Logo.png");
            //Image iconImg = Image.FromStream(imgStream);
            //showPane.LargeImage = Utilities.ToImageSource(iconImg, ImageFormat.Png);
            PushButtonData showPane = new PushButtonData("Start App", "Start App", thisAssembly, "RevitDataExtractor.WebCommand");
            
            RibbonItem show = ribbon.AddItem(showPane);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}