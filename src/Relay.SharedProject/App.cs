using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Windows;
using Dynamo.Utilities;
using ProtoCore.AST.ImperativeAST;
using Relay.Utilities;
using UIFramework;
using ComboBox = Autodesk.Revit.UI.ComboBox;
using RibbonItem = Autodesk.Revit.UI.RibbonItem;

namespace Relay
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            // parse the location for the potential tab name
            Globals.PotentialTabDirectories = Directory.GetDirectories(Globals.ExecutingPath);
            
            //if (Globals.PotentialTabDirectories.Any())
            //{
            //    var potentialTabNames =
            //        Globals.PotentialTabDirectories.Select(d => new DirectoryInfo(d).Name).ToArray();
            //    // Use the first folder for this first ribbon
            //    Globals.RibbonTabName = potentialTabNames.First();
            //}
            //else
            //{
                Globals.RibbonTabName = "Hoare Lea";
            //}

			Globals.Discipline = "General";

			// subscribe to ribbon click events
			Autodesk.Windows.ComponentManager.UIElementActivated += ComponentManagerOnUIElementActivated;
            // Attach custom event handler
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            CreateRibbon(a);
            a.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;

            return Result.Succeeded;
        }

        private void ControlledApplication_ApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            var syncGraphsId = RevitCommandId.LookupCommandId("CustomCtrl_%CustomCtrl_%Relay%Setup%SyncGraphs");

            if (sender is UIApplication uiapp)
            {
                RibbonUtils.SyncGraphs(uiapp);
            }
            else
            {
                uiapp = new UIApplication(sender as Application);
                RibbonUtils.SyncGraphs(uiapp);
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public void CreateRibbon(UIControlledApplication a)
        {
            try
            {
                // Create a custom ribbon tab
                a.CreateRibbonTab(Globals.RibbonTabName);
            }
            catch
            {
                // Might Already Exist
            }


            //add our setup panel and button
            var setupRibbonPanel = a.CreateRibbonPanel(Globals.RibbonTabName, "Relay");

			// Create Combobox
			string localMechImage = Path.Combine(Globals.RelayGraphs, "mech_16.png");
			string localElecImage = Path.Combine(Globals.RelayGraphs, "elec_16.png");
			string localPHImage = Path.Combine(Globals.RelayGraphs, "ph_16.png");
			string localGeneralImage = Path.Combine(Globals.RelayGraphs, "gen_16.png");

			BitmapImage mechImage = new BitmapImage( new Uri(localMechImage));
			BitmapImage elecImage = new BitmapImage( new Uri(localElecImage));
			BitmapImage phImage = new BitmapImage( new Uri(localPHImage ));
			BitmapImage generalImage = new BitmapImage( new Uri(localGeneralImage));

			ComboBoxData comboBoxData = new ComboBoxData("Select Discipline");
			comboBoxData.ToolTip = "Select your discipline from the dropdown";
			comboBoxData.Name = "Select Discipline";

			IList<ComboBoxMemberData> comboBoxMemberDataList = new List<ComboBoxMemberData>();

			ComboBoxMemberData mechComboBoxMemberData = new ComboBoxMemberData("Mechanical", "Mechanical");
			mechComboBoxMemberData.ToolTip = "Mechanical Scripts";
			mechComboBoxMemberData.ToolTipImage = ImageUtils.LoadImage(Globals.ExecutingAssembly, localMechImage);
			mechComboBoxMemberData.Image = new BitmapImage(new Uri(localMechImage));

			ComboBoxMemberData elecComboBoxMemberData = new ComboBoxMemberData("Electrical", "Electrical");
			elecComboBoxMemberData.ToolTip = "Electrical Scripts";
			elecComboBoxMemberData.ToolTipImage = ImageUtils.LoadImage(Globals.ExecutingAssembly, localElecImage);
			elecComboBoxMemberData.Image = new BitmapImage(new Uri(localElecImage));

			ComboBoxMemberData PHComboBoxMemberData = new ComboBoxMemberData("Public Health", "Public Health");
			PHComboBoxMemberData.ToolTip = "Public Health Scripts";
			PHComboBoxMemberData.ToolTipImage = ImageUtils.LoadImage(Globals.ExecutingAssembly, localPHImage);
			PHComboBoxMemberData.Image = new BitmapImage(new Uri(localPHImage));

			ComboBoxMemberData genComboBoxMemberData = new ComboBoxMemberData("General", "General");
			genComboBoxMemberData.ToolTip = "General and model manager Scripts";
			genComboBoxMemberData.ToolTipImage = ImageUtils.LoadImage(Globals.ExecutingAssembly, localGeneralImage);
			genComboBoxMemberData.Image = new BitmapImage(new Uri(localGeneralImage));

			comboBoxMemberDataList.Add(genComboBoxMemberData);
			comboBoxMemberDataList.Add(mechComboBoxMemberData);
			comboBoxMemberDataList.Add(elecComboBoxMemberData);
			comboBoxMemberDataList.Add(PHComboBoxMemberData);


			//if the sync exists in the relay graphs location, use it, if not use the resource
			string localSyncImage = Path.Combine(Globals.RelayGraphs, "Sync_16.png");
            BitmapImage syncImage = File.Exists(localSyncImage) ? new BitmapImage(new Uri(localSyncImage)) : ImageUtils.LoadImage(Globals.ExecutingAssembly, "Sync_16.png");

            PushButtonData syncButtonData = new PushButtonData("SyncGraphs", "Refresh",
                Path.Combine(Globals.ExecutingPath, "Relay.dll"), "Relay.RefreshGraphs")
            {
                Image = syncImage,
                ToolTip = "This will sync graphs from the default graph directory. Hold down left shift key to force large images"
            };

			IList<RibbonItem> ribbonITems =  setupRibbonPanel.AddStackedItems(comboBoxData, syncButtonData);
			
			ComboBox comboBox = ribbonITems[0] as Autodesk.Revit.UI.ComboBox;
			comboBox.AddItems(comboBoxMemberDataList);
			comboBox.CurrentChanged += new EventHandler<Autodesk.Revit.UI.Events.ComboBoxCurrentChangedEventArgs>(changeItem);
		}
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Get assembly name
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";

            // Get resource name
            var resourceName = Globals.EmbeddedLibraries.FirstOrDefault(x => x.EndsWith(assemblyName));
            if (resourceName == null)
            {
                return null;
            }

            // Load assembly from resource
            using (var stream = Globals.ExecutingAssembly.GetManifestResourceStream(resourceName))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }

        private void ComponentManagerOnUIElementActivated(object sender, UIElementActivatedEventArgs e)
        {
            if(e.Item is null ) return;
            try
            {
                if (!e.Item.Id.Contains("relay")) return;
                //set our current graph based on the click on the ribbon item
                Globals.CurrentGraphToRun = e.Item.Description.GetStringBetweenCharacters('[', ']');
            }
            catch (Exception)
            {
                // suppress the error if it happens
            }
        }

		private void changeItem( object sender, ComboBoxCurrentChangedEventArgs args )
		{
			try
			{
				Globals.Discipline = args.NewValue.Name;
				Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
				foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
				{
					if (tab.Title == "Hoare Lea")
					{
						foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
						{
							string oldDiscipline = args.OldValue.Name;
							string newDiscipline = args.NewValue.Name;
							string[] newPaneldirectories = Directory.GetDirectories(Path.Combine(Globals.ExecutingPath,"Hoare Lea", newDiscipline),"*", SearchOption.AllDirectories);
							string[] allPaneldirectories = Directory.GetDirectories(Path.Combine(Globals.ExecutingPath,"Hoare Lea"),"*", SearchOption.AllDirectories);
							if (newPaneldirectories.DefaultIfEmpty("empty").FirstOrDefault(e => e.Contains(panel.Source.Id) == true) != "empty")
							{
								panel.IsVisible = true;
							}
							else if(allPaneldirectories.Contains(panel.Source.Id))
							{
								panel.IsVisible = false;
							}
							if (panel.Source.Id.Contains("Relay"))
							{
								RibbonItemCollection collctn=panel.Source.Items;

								foreach (Autodesk.Windows.RibbonItem ri in collctn)
								{

									ri.Width = 20;
								}
							}
							RibbonUtils.SyncGraphs(args.Application);
						}
					}
				}

			}
			catch (Exception)
			{
				// suppress the error if it happens
			}
		}

	}
}
