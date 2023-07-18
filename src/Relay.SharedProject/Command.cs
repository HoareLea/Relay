using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Relay.Utilities;

namespace Relay
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class RefreshGraphs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;

			RibbonUtils.SyncGraphs(uiapp);

			return Result.Succeeded;
        }

       
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Run : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string dynamoJournal = Globals.CurrentGraphToRun;

            if (string.IsNullOrWhiteSpace(dynamoJournal))
            {
                return Result.Failed;
            }
            
            return Methods.DynamoMethods.RunGraph(commandData.Application, dynamoJournal);
        }
    }
}
