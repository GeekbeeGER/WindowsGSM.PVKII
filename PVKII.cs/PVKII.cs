﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;
using System.Linq;
using System.Net;
 
 
 
namespace WindowsGSM.Plugins
{
    public class PVKII  : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.PVKII ", // WindowsGSM.XXXX
            author = "GeekbeeGER",
            description = "WindowsGSM plugin for supporting PVKII  Dedicated Server",
            version = "1.0",
            url = "https://github.com/GeekbeeGER/WindowsGSM.PVKII ", // Github repository link (Best practice)
            color = "#34c9eb" // Color Hex
        };
 
        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "17575"; // Game server appId
 
        // - Standard Constructor and properties
        public PVKII (ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;
 
 
        // - Game server Fixed variables
        public override string StartPath => @"srcds.exe"; // Game server start path
        public string FullName = "PVKII Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 10; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = null; // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()
 
 
        // - Game server default values
        public string Port = "27070"; // Default port
        public string QueryPort = "27080"; // Default query port
        public string Defaultmap = "bt_island"; // Default map name
        public string Maxplayers = "32"; // Default maxplayers
        public string Additional = "-nographics -console -steam -game pvkii"; // Additional server start parameter
 
 
        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            //No config file seems
        }
 
        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            
            string ExePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(ExePath))
            {
                Error = $"{Path.GetFileName(ExePath)} not found ({ExePath})";
                return null;
            }           
            
        
 
            // Prepare start parameter
 
            string param = $"-batchmode server {_serverData.ServerParam}" + (!AllowsEmbedConsole ? " -log" : string.Empty);    
    
 
 
            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = ExePath,
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = true
                },
                EnableRaisingEvents = true
            };
 
            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
 
                // Start Process
                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }
 
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }
 
            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


// - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                 Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                 Functions.ServerConsole.SendWaitToMainWindow("^c");
            });
             await Task.Delay(500);
        }
 
    }
}
