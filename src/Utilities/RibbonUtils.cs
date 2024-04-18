﻿using System.IO;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using AW = Autodesk.Windows;
using System.Text.Json;
using System.Windows.Input;
using Relay.Classes;
using System.Reflection;

namespace Relay.Utilities
{
    class RibbonUtils
    {

        public static List<List<T>> SplitList<T>(List<T> me, int size = 50)
        {
            var list = new List<List<T>>();
            for (int i = 0; i < me.Count; i += size)
                list.Add(me.GetRange(i, Math.Min(size, me.Count - i)));
            return list;
        }

        public static void AddItems(Autodesk.Revit.UI.RibbonPanel panelToUse, string[] dynPaths, bool forceLargeIcon = false)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var totalFiles = dynPaths.Length;

            List<PushButtonData> pushButtonDatas = new List<PushButtonData>();
            foreach (var file in dynPaths)
            {
                //generate file info to get the datas
                FileInfo fInfo = new FileInfo(file);

                string tooltip = GetDescription(fInfo);

                string buttonName = $"relay{fInfo.Name.Replace(" ", "")}?{Guid.NewGuid()}";

                PushButtonData newButtonData = new PushButtonData(buttonName,
                    fInfo.Name.GenerateButtonText(),
                    Path.Combine(Globals.ExecutingPath, "Relay.dll"), "Relay.Run")
                {
                    ToolTip = tooltip
                };
              
                //set the images, if there are none, use default
                string icon32 = fInfo.FullName.Replace(".dyn", "_32.png");

                //trying out using temp icons to enable button hiding and deleting later
                if (File.Exists(icon32))
                {
                    var temp32 = Path.Combine(Globals.UserTemp, $"{Guid.NewGuid().ToString()}.png");
                    File.Copy(icon32, temp32);
                    icon32 = temp32;
                }
               
                newButtonData.LargeImage = File.Exists(icon32)
                    ? new BitmapImage(new Uri(icon32))
                    : ImageUtils.LoadImage(assembly, "Dynamo_32.png");

                string icon16 = fInfo.FullName.Replace(".dyn", "_16.png");

                //trying out using temp icons to enable button hiding and deleting later
                if (File.Exists(icon16))
                {
                    var temp16 = Path.Combine(Globals.UserTemp, $"{Guid.NewGuid().ToString()}.png");
                    File.Copy(icon16, temp16);
                    icon16 = temp16;
                }

                newButtonData.Image = File.Exists(icon16)
                    ? new BitmapImage(new Uri(icon16))
                    : ImageUtils.LoadImage(assembly, "Dynamo_16.png");

                TrySetContextualHelp(newButtonData, fInfo);

                pushButtonDatas.Add(newButtonData);
            }

            if (!panelToUse.Visible)
            {
                panelToUse.Visible = true;
            }

            if (forceLargeIcon)
            {
                foreach (var pushButton in pushButtonDatas)
                {
                    panelToUse.AddItem(pushButton);
                }
                return;
            }

            var splitButtons = SplitList(pushButtonDatas, 2);

            foreach (var buttonGroup in splitButtons)
            {
                switch (buttonGroup.Count)
                {
                    case 2:
                        var stack = panelToUse.AddStackedItems(buttonGroup[0], buttonGroup[1]);
                        break;
                    case 1:
                        panelToUse.AddItem(buttonGroup[0]);
                        break;
                }
            }
        }

        public static void HideUnused()
        {
            AW.RibbonControl ribbon = AW.ComponentManager.Ribbon;

            foreach (AW.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == Globals.RibbonTabName)
                {
                    foreach (var ribbonPanel in tab.Panels)
                    {
                        if (ribbonPanel.Source.Title != "Setup")
                        {
                            //hide every button that no longer has a dyn backing it
                            foreach (var ribbonItem in ribbonPanel.Source.Items)
                            {
                               if(ribbonItem.Description is null) continue;
                               AW.RibbonItem item = ribbonItem;
                               if (ribbonItem is AW.RibbonButton ribbonButton)
                               {
                                  item = ribbonButton;
                               }

                               
                                var dynPath = ribbonItem.Description.GetStringBetweenCharacters('[', ']');

                                if (!File.Exists(dynPath))
                                {
                                    item.IsVisible = false;
                                }
                                
                            }

                            //now hide the panel if all the buttons are gone
                            if (ribbonPanel.Source.Items.All(i => !i.IsVisible))
                            {
                                ribbonPanel.IsVisible = false;
                            }
                         
                        }
                    }
                }
            }
        }
        public static void ClearRibbon()
        {
            AW.RibbonControl ribbon = AW.ComponentManager.Ribbon;

            foreach (AW.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == Globals.RibbonTabName)
                {
                    foreach (var ribbonPanel in tab.Panels)
                    {
                        if (ribbonPanel.Source.Title != "Setup")
                        {
                            //hide every button
                            foreach (var ribbonItem in ribbonPanel.Source.Items)
                            {
                                ribbonItem.IsVisible = false;
                            }

                            //now hide the panel
                            ribbonPanel.IsVisible = false;
                        }
                    }
                }
            }
        }

        private static string GetDescription(FileInfo fInfo)
        {
            string description;
            try
            {
                string jsonString = File.ReadAllText(fInfo.FullName);
                var relayGraph = JsonSerializer.Deserialize<RelayGraph>(jsonString);

                description = relayGraph.Description != null ? $"{relayGraph.Description}\r\r[{fInfo.FullName}]" : $"[{fInfo.FullName}]";
            }
            catch (Exception e)
            {
                var s = e.Message;
                description = $"[{fInfo.FullName}]";
            }
           
            return description;
        }

        private static void TrySetContextualHelp(PushButtonData pushButtonData, FileInfo fInfo)
        {
            try
            {
                string jsonString = File.ReadAllText(fInfo.FullName);
                var relayGraph = JsonSerializer.Deserialize<RelayGraph>(jsonString);

                if (relayGraph.GraphDocumentationURL is null || string.IsNullOrWhiteSpace(relayGraph.GraphDocumentationURL))
                {
                    return;
                }

                ContextualHelp help = new ContextualHelp(ContextualHelpType.Url, relayGraph.GraphDocumentationURL);
                pushButtonData.SetContextualHelp(help);
            }
            catch (Exception)
            {
                //don't set the help
            }
        }

        public static AW.RibbonItem GetButton(string tabName, string panelName, string itemName)
        {
            AW.RibbonControl ribbon = AW.ComponentManager.Ribbon;

            foreach (AW.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    foreach (AW.RibbonPanel panel in tab.Panels)
                    {
                        if (panel.Source.Title == panelName)
                        {
                            AW.RibbonItem ribbonItem = null;
                            foreach (var item in panel.Source.Items)
                            {
                                if (item is AW.RibbonRowPanel ribbonPanel)
                                {
                                    foreach (var rItem in ribbonPanel.Items)
                                    {
                                        if (rItem.Id.Contains(itemName))
                                        {
                                            ribbonItem = rItem;
                                            break;
                                        }
                                    }
                                }

                                if (item.Id.Contains(itemName))
                                {
                                    ribbonItem = item;
                                    break;
                                }
                            }

                            if (ribbonItem == null) return null;

                            if (!ribbonItem.IsVisible)
                            {
                                return null;
                            }

                            return ribbonItem;
                        }
                    }
                }
            }
            return null;
        }

        public static AW.RibbonPanel GetPanel(string tabName, string panelName)
        {
            AW.RibbonControl ribbon = AW.ComponentManager.Ribbon;

            foreach (AW.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    foreach (AW.RibbonPanel panel in tab.Panels)
                    {
                        if (panel.Source.Title == panelName)
                        {
                            return panel;
                        }
                    }
                }
            }
            return null;
        }
        public static AW.RibbonTab GetTab(string tabName)
        {
            AW.RibbonControl ribbon = AW.ComponentManager.Ribbon;

            foreach (AW.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    return tab;
                }
            }
            return null;
        }
     
        public static void SyncGraphs(UIApplication uiapp)
        {
            // rescan the potential tab directory
            Globals.PotentialTabDirectories = Directory.GetDirectories(Globals.BasePath);

            // no tabs found, exit
            if (!Globals.PotentialTabDirectories.Any())
            {
                return;
            }

            // iterate through all sub folders
            foreach (var potentialTabDirectory in Globals.PotentialTabDirectories)
            {
                // current tab name
                string potentialTab = new DirectoryInfo(potentialTabDirectory).Name;

                try
                {
                    // Create a custom ribbon tab
                    uiapp.CreateRibbonTab(potentialTab);
                }
                catch
                {
                    // Might Already Exist
                }


                //check if a specific Revit version directory exists, if it does use that.
                string[] graphDirectories = Directory.GetDirectories(potentialTabDirectory);
                var versionDirectories = Directory.GetDirectories(potentialTabDirectory);

                if (versionDirectories.Any(d => new DirectoryInfo(d).Name.Contains(Globals.RevitVersion)))
                {
                    var versionDirectory = versionDirectories.FirstOrDefault(d => new DirectoryInfo(d).Name.Contains(Globals.RevitVersion));

                    if (versionDirectory != null)
                    {
                        graphDirectories = Directory.GetDirectories(versionDirectory);
                    }
                }

                //create the panels for the sub directories
                foreach (var directory in graphDirectories)
                {
                    //the upper folder name (panel name)
                    DirectoryInfo dInfo = new DirectoryInfo(directory);

                    Autodesk.Revit.UI.RibbonPanel panelToUse;

                    //try to create the panel, if it already exists, just use it
                    try
                    {
                        panelToUse = uiapp.CreateRibbonPanel(potentialTab, dInfo.Name);
                    }
                    catch (Exception)
                    {
                        panelToUse = uiapp.GetRibbonPanels(potentialTab).First(p => p.Name.Equals(dInfo.Name));
                    }

                    //find the files that do not have a button yet
                   
                    List<string> toCreate = new List<string>();

                    foreach (var file in Directory.GetFiles(directory, "*.dyn"))
                    {
                        if (RibbonUtils.GetButton(potentialTab, dInfo.Name, $"relay{new FileInfo(file).Name.Replace(" ", "")}") == null)
                        {
                            toCreate.Add(file);
                        }
                    }


                    //if the user is holding down the left shift key, then force the large icons
                    bool forceLargeIcons = Keyboard.IsKeyDown(Key.LeftShift);

                    if (toCreate.Any())
                    {
                        RibbonUtils.AddItems(panelToUse, toCreate.ToArray(), forceLargeIcons);

                    }
                }
            }
        }

    }
}
