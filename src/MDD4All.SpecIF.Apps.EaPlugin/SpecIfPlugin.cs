using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.Apps.EaPlugin.ViewModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.MongoDB;
using System.Collections.Generic;
using System.Net;
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

        private const string ADD_REQIOREMENT_TO_SPECIF_REPOSITORY_MENU = "Add requirement to SpecIF repository";

        private const string ADD_SPECIFICATION_TO_SPECIF_MENU = "Add specification to SpecIF repository";

        private const string ABOUT_MENU = "&About FMC4SE...";

        private MainViewModel _mainViewModel;

        private string _specIfURL = "";

        

        public string EA_Connect(EAAPI.Repository repository)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            

            return "";
        }

        public void EA_FileOpen(EAAPI.Repository repository)
        {
            ISpecIfMetadataReader metadataReader = new SpecIfMongoDbMetadataReader("mongodb://localhost:27017");

            _mainViewModel = new MainViewModel(repository, metadataReader);

            _specIfURL = _mainViewModel.GetSpecIfRepositoryURL();
        }

        public object EA_GetMenuItems(EAAPI.Repository repository,
                                      string location, string menuName)
        {

            object result = "";

            List<string> menuEntries = new List<string>();

            object selectedItem = null;

            EAAPI.ObjectType objectType = repository.GetTreeSelectedItem(out selectedItem);

            EAAPI.Element selectedElement = null;

            if(objectType == EAAPI.ObjectType.otElement)
            {
                selectedElement = selectedItem as EAAPI.Element;
            }
            else if(objectType == EAAPI.ObjectType.otPackage)
            {
                EAAPI.Package package = selectedItem as EAAPI.Package;
                selectedElement = package.Element;
            }

            switch (menuName)
            {
                // defines the top level menu option
                case "":
                    result = MAIN_MENU;
                break;
                // defines the submenu options

                case MAIN_MENU:
                    if (location == "TreeView")
                    {
                        menuEntries.Add(EXPORT_MENU);
                        menuEntries.Add("-");
                        menuEntries.Add(SYNC_PROJECT_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_MENU);
                        
                        if(selectedElement != null && 
                            selectedElement.Type == "Requirement" && 
                            selectedElement.Stereotype == "fmcreq" &&
                            !string.IsNullOrEmpty(_specIfURL) &&
                            string.IsNullOrEmpty(selectedElement.GetTaggedValueString("specifId")))
                        {
                            menuEntries.Add("-");
                            menuEntries.Add(ADD_REQIOREMENT_TO_SPECIF_REPOSITORY_MENU);
                        }

                        if(objectType == EAAPI.ObjectType.otPackage &&
                           selectedElement != null &&
                           selectedElement.Stereotype == "specification")
                        {
                            menuEntries.Add("-");
                            menuEntries.Add(ADD_SPECIFICATION_TO_SPECIF_MENU);
                        }
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

                    case ADD_REQIOREMENT_TO_SPECIF_REPOSITORY_MENU:
                        _mainViewModel.AddSingleRequirementToSpecIfCommand.Execute(null);
                        break;

                    case ADD_SPECIFICATION_TO_SPECIF_MENU:
                        _mainViewModel.AddSpecificationToSpecIfCommand.Execute(null);
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

        public bool EA_OnPreNewElement(EAAPI.Repository repository, EAAPI.EventProperties properties)
        {
            bool result = true;

            /* properties:
              0: Type
              1: Stereotype
              2: ParentID
              3: DiagramID
              4: FQStereotype
             */

            

            return result;
        }

        public bool EA_OnPostNewElement(EAAPI.Repository repository, EAAPI.EventProperties properties)
        {
            bool result = false; // no data update here

            /* properties:
              0: ElementID
            */

            

            return result;
        }
    }
}
