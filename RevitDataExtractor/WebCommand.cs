#region Namespaces
using System;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Threading;
using Autodesk.Revit.UI.Selection;
using System.Threading.Tasks;
using WallDataPlugin;
#endregion

/// <summary>
/// For showing the Web UI window.
/// Author: Bob Lee
/// </summary>
namespace RevitDataExtractor
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WebCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements
            )
        {
            try
            {

                RevitHttpServer httpServer = new RevitHttpServer();
                httpServer.SetUIApplication(commandData.Application);
                httpServer.Start();

                UIApplication uiApp = commandData.Application;
                App app = App.GetAddInInstance(commandData.Application.ActiveAddInId);
                if (app != null && App.httpServer != null)
                {
                    App.httpServer.SetUIApplication(uiApp);
                }

                var webView2Control = new Microsoft.Web.WebView2.WinForms.WebView2();

                UserForm userForm = new UserForm(uiApp, webView2Control);
                userForm.ShowDialog();
                string url = userForm.WebLink;

                WebWindow webWindow = new WebWindow();
                //App.rvtHandler.webWindow = webWindow;
                webWindow.Show();

                var wallData = new WallDataExporter().CollectWallData(commandData.Application.ActiveUIDocument.Document);
                Task.Run(() => new WallDataExporter().SendMessageAsync(wallData));

                return Result.Succeeded;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                return Result.Failed;
            }
        }
    }
}