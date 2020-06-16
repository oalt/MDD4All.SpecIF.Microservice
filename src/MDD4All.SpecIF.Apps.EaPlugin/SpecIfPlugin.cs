using MDD4All.SpecIF.Apps.EaPlugin.ViewModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaPlugin
{
    public class SpecIfPlugin
    {
        private const string MAIN_MENU = "-&SpecIF";

        private const string EXPORT_MENU = "&Export to SpecIF...";

        private const string SYNC_PROJECT_ROOTS_MENU = "&Synchronize project roots";

        private const string SYNC_HIERARHY_ROOTS_MENU = "&Synchronize hierarchy roots";

        private const string SYNC_HIERARHY_MENU = "&Synchronize hierarchy";

        private const string ABOUT_MENU = "&About FMC4SE...";

        private MainViewModel _mainViewModel;

        public string EA_Connect(EAAPI.Repository repository)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            ISpecIfMetadataReader metadataReader = new SpecIfMongoDbMetadataReader("mongodb://localhost:27017");

            _mainViewModel = new MainViewModel(repository, metadataReader);

            return "";
        }

        public object EA_GetMenuItems(EAAPI.Repository Repository,
                                      string Location, string MenuName)
        {

            object result = "";

            List<string> menuEntries = new List<string>();

            switch (MenuName)
            {
                // defines the top level menu option
                case "":
                    result = MAIN_MENU;
                break;
                // defines the submenu options

                case MAIN_MENU:
                    if (Location == "TreeView")
                    {
                        menuEntries.Add(EXPORT_MENU);
                        menuEntries.Add("-");
                        menuEntries.Add(SYNC_PROJECT_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_MENU);
                        
                    }
                    result = menuEntries.ToArray();
                break;
            }

            return result;
        }

        public void EA_GetMenuState(EAAPI.Repository Repository, 
                                    string Location, 
                                    string MenuName, 
                                    string ItemName, 
                                    ref bool IsEnabled, 
                                    ref bool IsChecked)
        {
            IsEnabled = false;

            switch (ItemName)
            {

                case EXPORT_MENU:
                    IsEnabled = true;
                    break;

                default:
                    IsEnabled = true;
                    break;
            }


        }

        public void EA_MenuClick(EAAPI.Repository Repository, string Location, string MenuName, string ItemName)
        {
            if (_mainViewModel != null)
            {

                switch (ItemName)
                {

                    case EXPORT_MENU:
                        _mainViewModel.ExportToSpecIfCommand.Execute(null);
                        break;

                    case SYNC_PROJECT_ROOTS_MENU:
                        _mainViewModel.SynchronizeProjectRootsCommand.Execute(null);
                        break;

                    case SYNC_HIERARHY_ROOTS_MENU:
                        _mainViewModel.SynchronizeProjectHierarchyRootsCommand.Execute(null);
                        break;

                    case SYNC_HIERARHY_MENU:
                        _mainViewModel.SynchronizeHierarchyResourcesCommand.Execute(null);
                        break;

                    default:

                        break;
                }
            }

        }

        public void EA_OnNotifyContextItemModified(EAAPI.Repository repository, string guid, EAAPI.ObjectType objectType)
        {
            ;
        }
    }
}
