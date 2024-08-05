using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Relay.Utilities.RibbonUtils;

namespace Relay.Utilities
{
    public partial class Globals
    {
        public static readonly string Version =
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string ExecutingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string UserTemp = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);
        public static string RevitVersion { get; set; }
        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        public static string[] EmbeddedLibraries = ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();
        public static string[] PotentialTabDirectories { get; set; }
		public static string Discipline { get; set; }
		public static string RibbonTabName = "Hoare Lea";
        public static string RelayGraphs = Path.Combine(Globals.ExecutingPath, RibbonTabName);
		public static string GraphDetailPath = Path.Combine(Globals.ExecutingPath, RibbonTabName,"file_settings.json");
		public static IList<DynamoGraphFileInfo> GraphFileInfo { get; set; }

		public static string CurrentGraphToRun { get; set; } = "";

		public class DynamoGraphFileInfo
		{
			[JsonProperty("fileName")]
			public string fileName { get; set; }
			[JsonProperty("dynamoName")]
			public string dynamoName { get; set; }
			[JsonProperty("tooltip")]
			public string tooltip { get; set; }
			[JsonProperty("discipline")]
			public string discipline { get; set; }
		}

	}
}
﻿using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;

namespace Relay.Utilities
{
    public partial class Globals
    {
        public static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        public static readonly string Version = ExecutingAssembly.GetName().Version.ToString();

        public static string ExecutingPath = Path.GetDirectoryName(ExecutingAssembly.Location);
        public static string BasePath { get; set; } = ExecutingPath;

        public static string UserTemp = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);
        public static string RevitVersion { get; set; }
      
        public static string[] EmbeddedLibraries = ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();
        public static string[] PotentialTabDirectories { get; set; }

        public static string RibbonTabName { get; set; } = "Relay";
        public static string RelayGraphs = Path.Combine(ExecutingPath, RibbonTabName);

        public static string CurrentGraphToRun { get; set; } = "";


        public static Dictionary<string, RibbonItem> RelayButtons = new Dictionary<string, RibbonItem>();
        public static Dictionary<string, List<RibbonItem>> RelayPanels = new Dictionary<string, List<RibbonItem>>();
    }
}
