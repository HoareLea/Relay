using System.Collections.Generic;
using Autodesk.Revit.UI;
using Dynamo.Applications;
using Relay.Utilities;

namespace Relay.Methods
{
    internal static class DynamoMethods
    {
        internal static Result RunGraph(UIApplication app, string dynamoJournal)
        {
			bool continueOn = false;
			try
			{
				continueOn = DynamoUtils.SetToAutomatic(dynamoJournal);
			}
			//toggle the graph to automatic. this is required for running Dynamo UI-Les
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
				return Result.Failed;
			}

			if(continueOn)
			{ 
				DynamoRevit dynamoRevit = new DynamoRevit();

				IDictionary<string, string> journalData = new Dictionary<string, string>
				{
					{JournalKeys.ShowUiKey, false.ToString()},
					{JournalKeys.AutomationModeKey, true.ToString()},
					{JournalKeys.DynPathKey, ""},
					{JournalKeys.DynPathExecuteKey, true.ToString()},
					{JournalKeys.ForceManualRunKey, false.ToString()},
					{JournalKeys.ModelShutDownKey, true.ToString()},
					{JournalKeys.ModelNodesInfo, false.ToString()},
				};
				DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData
				{
					Application = app,
					JournalData = journalData
				};

				var result = dynamoRevit.ExecuteCommand(dynamoRevitCommandData);

				//sorry folks, parks closed, the moose out front should have told you
				#if Revit2021Pro || Revit2022Pro || Revit2023Pro
							Packages.ResolvePackages(DynamoRevit.RevitDynamoModel.PathManager.DefaultPackagesDirectory, dynamoJournal);
				#endif

				DynamoRevit.RevitDynamoModel.OpenFileFromPath(dynamoJournal, true);
				DynamoRevit.RevitDynamoModel.ForceRun();

				return result;
			}
			else
			{
				return Result.Failed;
			}
		}
    }
}
