using MDD4All.SpecIF.Apps.EaPlugin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaPlugin
{
    public class SpecIfPlugin
    {
        private const string MAIN_MENU = "-&SpecIF";

        private const string EXPORT_MENU = "&Export to SpecIF...";
        private const string ABOUT_MENU = "&About FMC4SE...";

        private MainViewModel _mainViewModel;

        public string EA_Connect(EAAPI.Repository repository)
        {
            _mainViewModel = new MainViewModel(repository);

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
                    IsEnabled = false;
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

                    default:

                        break;
                }
            }

        }
    }
}
