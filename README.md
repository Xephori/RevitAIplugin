# Revit Web App Boilerplate
Boilerplate .NET solution for Revit Web Apps. Built using Revit Plugin wizard for Revit 2022.

## Requirements
- Visual Studio Express 2017 or later

## Dependencies
- Microsoft.Web.WebView2 < v1.0.1938.49
- Newtonsoft.Json
- CsvHelper

## References
- PresentationCore
- PresentationFramework
- System.Drawing
- System.Xaml
- System.Windows
- RevitAPI
- RevitAPIUI
- WindowsBase

## Important
After unzip/cloning the repository to local folders, search for this file --> RevitDataExtractor.addin Cut and paste the file in your revit addin folder. Usually, this folder will be located here --> "C:\Users"your username here"\AppData\Roaming\Autodesk\Revit\Addins\2024" Note* Your revit version may not be 2024 as shown in the path above. Select the correct revit version you want to install the plug-in instead. Open the "RevitDataExtractor.addin" file using a notepad and edit the following line --> <assembly> [RevitDataExtractor.RevitDataExtractor.addin](https://github.com/Xephori/RevitAIplugin/blob/streamlit/RevitDataExtractor/RevitDataExtractor.addin)

Instead of loading the web ui from streamlit, you may replace [RevitDataExtractor.LaunchWeb.cs:32](https://github.com/Xephori/RevitAIplugin/blob/streamlit/RevitDataExtractor/LaunchWeb.cs) with a url of your development server, to allow faster development (just refresh to show changes to the ui). 

## How to run
1. Go to Visual Studio Code Terminal 
2. Run the following commands in order
    - `cd python`
    - `venv/Scripts/activate` 
    - `streamlit run streamlit_app.py` 
3. Our url on LaunchWeb will automatically run on `http://localhost:8501`.
4. Then if necessary, build the solution in Visual Studio.
5. Open the plugin in Revit and ta-da!

# How it works
1. Revit opens up the streamlit webpage in your default browser.
2. Streamlit has buttons to api calls to localhost run from Revit to get data to work.
3. Streamlit runs the backend functions (chatbot and processing of data).

