﻿using Newtonsoft.Json;
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
