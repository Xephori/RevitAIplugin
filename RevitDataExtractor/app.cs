#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Windows;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using WallDataPlugin;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Media;
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
                string tabName = "AI Plugin";
                string ribbonPanelName = "AI Tools";

                // Create a new ribbon tab
                a.CreateRibbonTab(tabName);

                // Create a new ribbon panel within the tab
                RibbonPanel ribbonPanel = a.CreateRibbonPanel(tabName, ribbonPanelName);

                string thisAssembly = Assembly.GetExecutingAssembly().Location;

                PushButtonData buttonData = new PushButtonData(
                    "RevitAIPlugin",
                    "Launch AI Plugin",
                    thisAssembly,
                    "RevitDataExtractor.LaunchCommand"
                );

                // Set the icon for the button
                string iconPath = Path.Combine(Directory.GetCurrentDirectory(), "icon.jpg");
                if (File.Exists(iconPath))
                {
                    buttonData.LargeImage = BitmapToImageSource(new Bitmap(iconPath));
                }

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

        public static App GetAddInInstance(AddInId addInId)
        {
            // Implement logic to return the instance of App associated with the given AddInId
            // This is a placeholder implementation and should be replaced with actual logic
            return new App();
        }

        public static void Raise(Action action)
        {
            if (action == null) return;

            try
            {
                // Ensure Revit's API threading rules are respected.
                action.Invoke();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }
        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private static void LogError(Exception ex)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RevitPluginError.log");
            File.AppendAllText(logPath, $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n");
        }
    }

    [Transaction(TransactionMode.Manual)]
    class LaunchCommand : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UserForm userForm = new UserForm(uiApp, new Microsoft.Web.WebView2.WinForms.WebView2());
            userForm.ShowDialog();
            return Result.Succeeded;
        }
    }
}