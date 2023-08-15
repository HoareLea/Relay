using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.UI;
using Dynamo.Applications;

namespace Relay.Utilities
{
    class DynamoUtils
    {
        public static bool SetToAutomatic(string filePath)
        {
			if(filePath != null)
			{
				string text = "";
				try
				{
					text = File.ReadAllText(filePath);
					
					text = text.Replace(@"""RunType"": ""Manual"",", @"""RunType"": ""Automatic"",");

					if (text.Contains("\"IsSetAsInput\": true") || text == "")
					{
						// Creates a Revit task dialog to communicate information to the user.
						TaskDialog mainDialog = new TaskDialog("Graph Issue");
						if (text.Contains("\"IsSetAsInput\": true"))
						{
							mainDialog.MainInstruction = "Check Graph for Inputs!";
							mainDialog.MainContent =
									"Relay does not allow for User inputs in the same way as the Dynamo Player."
									+ " Please change them to use datashapes.";
						}
						else
						{
							mainDialog.MainInstruction = "Check Graph in dynamo!";
							mainDialog.MainContent =
									"Something is wrong with the file, please check relay folders"
									+ " and open the graph in dynamo to review.";
						}


						// Set footer text. Footer text is usually used to link to the help document.
						mainDialog.FooterText = "Contact DE for more information";

						TaskDialogResult tResult = mainDialog.Show();
						return false;
					}

					File.WriteAllText(filePath, text);
					return true;
				}
				catch
				{
					TaskDialog mainDialog = new TaskDialog("Graph Issue");
					
					mainDialog.MainInstruction = "Check Graph in dynamo!";
					mainDialog.MainContent =
							"Something is wrong with the file, please check relay folders"
							+ " and open the graph in dynamo to review.";
					
					// Set footer text. Footer text is usually used to link to the help document.
					mainDialog.FooterText = "Contact DE for more information";

					TaskDialogResult tResult = mainDialog.Show();
					return false;
				}
				
				
			}
			return false;
        }

        public static void InitializeDynamoRevit(ExternalCommandData commandData)
        {
            DynamoRevit dynamoRevit = new DynamoRevit();

            IDictionary<string, string> journalData = new Dictionary<string, string>
            {
                {JournalKeys.ShowUiKey, false.ToString()},
                {JournalKeys.AutomationModeKey, true.ToString()},
                {JournalKeys.DynPathExecuteKey, true.ToString()},
                {JournalKeys.ForceManualRunKey, false.ToString()},
                {JournalKeys.ModelShutDownKey, true.ToString()},
                {JournalKeys.ModelNodesInfo, false.ToString()},
            };
            DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData
            {
                Application = commandData.Application,
                JournalData = journalData
            };
            dynamoRevit.ExecuteCommand(dynamoRevitCommandData);
        }
    }
}
