// using System.Collections.Generic;
// using System.Diagnostics;
// using Autodesk.Revit.UI;
// using Autodesk.Revit.DB;
// using Autodesk.Revit.DB.Architecture;
// using System;
// using Newtonsoft.Json;

// /// <summary>
// /// For executing Revit Routines when called from Web UI
// /// Author: Bob Lee
// /// </summary>
// namespace RevitWebApp
// {
//     public class RevitEventHandler : IExternalEventHandler
//     {
//         public enum RevitActionsEnum
//         {
//             Invalid = -1,
//             Test,
//             GetVersion,
//             GetWallData, 
//         }

//         private RevitActionsEnum _currentRevitActions;
//         private readonly ExternalEvent _externalEvent;
//         public WebWindow webWindow;

//         public RevitEventHandler()
//         {
//             _externalEvent = ExternalEvent.Create(this);
//         }

//         public void Execute(UIApplication app)
//         {
//             Debug.WriteLine("Handling Revit Event!");
//             switch (_currentRevitActions)
//             {
//                 case RevitActionsEnum.Test:
//                     Debug.WriteLine("Msg received");
//                     // To send data back to the Web ui:
//                     webWindow.SendPayload("Test", "{\"msg\": \"msg from Revit!\"}");
//                     break;
//                 case RevitActionsEnum.GetVersion:
//                     Debug.WriteLine("Getting version...");
//                     var ver_num = app.Application.VersionNumber;
//                     webWindow.SendPayload("GetVersion", "{\"version\":"+ver_num+"}");
//                     break;
//                 case RevitActionsEnum.GetWallData: 
//                     Debug.WriteLine("Getting wall data...");
//                     var wallData = GetWallData(app);
//                     webWindow.SendPayload("GetWallData", wallData);
//                     break;
//                 default:
//                     Debug.WriteLine("Unhandled Action.");
//                     break;
//             }
//         }

//         private string GetWallData(UIApplication app)
//         {
//             var doc = app.ActiveUIDocument.Document;
//             var collector = new FilteredElementCollector(doc);
//             var walls = collector.OfClass(typeof(Wall)).ToElements();

//             var wallDataList = new List<Dictionary<string, string>>();

//             foreach (Wall wall in walls)
//             {
//                 var wallType = doc.GetElement(wall.GetTypeId()) as ElementType;
//                 var wallData = new Dictionary<string, string>
//                 {
//                     { "Id", wall.Id.ToString() },
//                     { "Name", wall.Name }
//                 };

//                 foreach (Parameter param in wallType.Parameters)
//                 {
//                     if (param.HasValue)
//                     {
//                         wallData[param.Definition.Name] = param.AsValueString();
//                     }
//                 }

//                 wallDataList.Add(wallData);
//             }

//             return JsonConvert.SerializeObject(wallDataList);
//         }

//         public ExternalEventRequest Raise(RevitActionsEnum revitActionsName) {
//             _currentRevitActions = revitActionsName;
//             return _externalEvent.Raise();
//         }

//         public string GetName()
//         {
//             return nameof(RevitEventHandler);
//         }

        
//     }
// }

using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using Newtonsoft.Json;

/// <summary>
/// For executing Revit Routines when called from Web UI
/// Author: Bob Lee
/// </summary>
namespace RevitWebApp
{
    public class RevitEventHandler : IExternalEventHandler
    {
        public enum RevitActionsEnum
        {
            Invalid = -1,
            Test,
            GetVersion,
            GetWallData,
        }

        private RevitActionsEnum _currentRevitActions;
        private readonly ExternalEvent _externalEvent;
        public WebWindow webWindow;

        public RevitEventHandler()
        {
            _externalEvent = ExternalEvent.Create(this);
            _currentRevitActions = RevitActionsEnum.Invalid;
        }

        public void Execute(UIApplication app)
        {
            UIApplication uiApp = app;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            switch (_currentRevitActions)
            {
                case RevitActionsEnum.Test:
                    // Existing Test action logic
                    TestFunction();
                    break;

                case RevitActionsEnum.GetVersion:
                    string version = GetRevitVersion();
                    // Store or send the version as needed
                    webWindow.SendResponse(new { Version = version });
                    break;

                case RevitActionsEnum.GetWallData:
                    List<string> wallTypes = GetWallElementTypes(doc);
                    // Store or send the wall types as needed
                    webWindow.SendResponse(new { WallTypes = wallTypes });
                    break;

                default:
                    Debug.Print("Invalid Revit Action");
                    break;
            }

            _currentRevitActions = RevitActionsEnum.Invalid;
        }

        public string GetName()
        {
            return "Revit External Event Handler";
        }

        /// <summary>
        /// Triggers an external event with the specified action.
        /// </summary>
        /// <param name="action">The Revit action to perform.</param>
        public void TriggerAction(RevitActionsEnum action)
        {
            if (action == RevitActionsEnum.Invalid)
            {
                Debug.Print("Invalid action. Cannot trigger.");
                return;
            }

            _currentRevitActions = action;
            _externalEvent.Raise();
        }

        /// <summary>
        /// Existing Test function.
        /// </summary>
        private void TestFunction()
        {
            // Existing Test logic
            Debug.Print("Test function executed.");
            webWindow.SendResponse(new { Message = "Test function executed." });
        }

        /// <summary>
        /// Retrieves the current Revit version.
        /// </summary>
        /// <returns>Revit version as a string.</returns>
        private string GetRevitVersion()
        {
            try
            {
                string version = Autodesk.Revit.ApplicationServices.Application.VersionName;
                return $"Revit Version: {version}";
            }
            catch (Exception ex)
            {
                Debug.Print($"Error retrieving Revit version: {ex.Message}");
                return "Error retrieving Revit version.";
            }
        }

        /// <summary>
        /// Retrieves all wall element types from the active Revit document.
        /// </summary>
        /// <param name="doc">Active Revit Document.</param>
        /// <returns>List of wall type names.</returns>
        private List<string> GetWallElementTypes(Document doc)
        {
            List<string> wallTypes = new List<string>();

            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(WallType));

                foreach (Element elem in collector)
                {
                    wallTypes.Add(elem.Name);
                }

                return wallTypes;
            }
            catch (Exception ex)
            {
                Debug.Print($"Error retrieving wall element types: {ex.Message}");
                return new List<string> { "Error retrieving wall types." };
            }
        }
    }

    /// <summary>
    /// Represents the web communication window.
    /// </summary>
    public class WebWindow
    {
        // Implement your web communication logic here.
        // This could involve HTTP clients, WebSockets, or other IPC mechanisms.

        /// <summary>
        /// Sends a JSON response back to the Web API or client.
        /// </summary>
        /// <param name="response">Anonymous object containing response data.</param>
        public void SendResponse(object response)
        {
            string jsonResponse = JsonConvert.SerializeObject(response);
            // Implement the logic to send this JSON back to the Web API,
            // e.g., via HTTP POST, WebSocket message, or another IPC mechanism.
            Debug.Print($"Response Sent: {jsonResponse}");
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class RevitController : ControllerBase
    {
        private readonly RevitEventHandler _revitEventHandler;

        public RevitController(RevitEventHandler revitEventHandler)
        {
            _revitEventHandler = revitEventHandler;
        }

        [HttpGet("GetVersion")]
        public IActionResult GetVersion()
        {
            _revitEventHandler.TriggerAction(RevitEventHandler.RevitActionsEnum.GetVersion);
            // Implement a mechanism to wait for the response, such as TaskCompletionSource
            // For simplicity, assume synchronous response
            return Ok(new { Message = "Version retrieval triggered." });
        }

        [HttpGet("GetWallElementTypes")]
        public IActionResult GetWallElementTypes()
        {
            _revitEventHandler.TriggerAction(RevitEventHandler.RevitActionsEnum.GetWallData);
            // Implement a mechanism to wait for the response
            return Ok(new { Message = "Wall data retrieval triggered." });
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            var app = builder.Build();
            app.MapControllers();
            app.Run("http://localhost:5000");
        }
    }

    public class RevitResponse
    {
        public string Version { get; set; }
        public List<string> WallTypes { get; set; }
    }
}
