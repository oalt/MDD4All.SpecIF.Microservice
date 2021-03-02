using GalaSoft.MvvmLight.Ioc;
using MDD4All.Configuration;
using MDD4All.Configuration.Contracts;
using MDD4All.EnterpriseArchitect.Logging;
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.Apps.EaPlugin.Configuration;
using MDD4All.SpecIF.Apps.EaPlugin.ViewModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.File;
using MDD4All.SpecIF.DataProvider.Jira;
using NLog;
using System.Collections.Generic;
using System.Net;
using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaPlugin
{
    public class SpecIfPlugin
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string MAIN_MENU = "-&SpecIF";

        //private const string EXPORT_MENU = "&Export to SpecIF...";

        private const string SYNC_PROJECT_ROOTS_MENU = "&Synchronize project roots";

        private const string SYNC_HIERARHY_ROOTS_MENU = "&Synchronize hierarchy roots";

        private const string SYNC_HIERARHY_MENU = "&Synchronize hierarchy";

        private const string ADD_REQIOREMENT_TO_SPECIF_REPOSITORY_MENU = "Add requirement to SpecIF repository";

        private const string ADD_SPECIFICATION_TO_SPECIF_MENU = "Add specification to SpecIF repository";

        private const string EDIT_SETTINGS_MENU = "&Edit Plugin Settings";

        private MainViewModel _mainViewModel;

        private SpecIfPluginConfiguration _configuration;

        private ISpecIfMetadataReader _specIfMetadataReader;
        private ISpecIfDataReader _specIfDataReader;
        private ISpecIfDataWriter _specIfRequirementDataWriter;

        public string EA_Connect(EAAPI.Repository repository)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            SimpleIoc.Default.Register<IConfigurationReaderWriter<SpecIfPluginConfiguration>, FileConfigurationReaderWriter<SpecIfPluginConfiguration>>();

            IConfigurationReaderWriter<SpecIfPluginConfiguration> configurationReaderWriter = SimpleIoc.Default.GetInstance<IConfigurationReaderWriter<SpecIfPluginConfiguration>>();

            if (configurationReaderWriter != null)
            {
                _configuration = configurationReaderWriter.GetConfiguration();

                if(_configuration == null)
                {
                    _configuration = new SpecIfPluginConfiguration();
                    configurationReaderWriter.StoreConfiguration(_configuration);
                }
            }

            _mainViewModel = new MainViewModel(repository, null, null, null);

            return "";
        }

        public void EA_FileOpen(EAAPI.Repository repository)
        {
            EaPluginNlogConfigurator.InitializePluginLogging(repository, "SpecIF");

            
            _specIfMetadataReader = new SpecIfFileMetadataReader(_configuration.SpecIfMetadataDirectory);
            _specIfDataReader = new SpecIfJiraDataReader(_configuration.JiraURL,
                                                         _configuration.JiraUserName,
                                                         _configuration.JiraApiKey,
                                                         _specIfMetadataReader);

            _specIfRequirementDataWriter = new SpecIfJiraDataWriter(_configuration.JiraURL,
                                                         _configuration.JiraUserName,
                                                         _configuration.JiraApiKey,
                                                         _specIfMetadataReader,
                                                         _specIfDataReader);

            _mainViewModel = new MainViewModel(repository, 
                                               _specIfMetadataReader,
                                               _specIfDataReader,
                                               _specIfRequirementDataWriter);
        }

        public object EA_GetMenuItems(EAAPI.Repository repository,
                                      string location, 
                                      string menuName)
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
                        //menuEntries.Add(EXPORT_MENU);
                        //menuEntries.Add("-");
                        menuEntries.Add(SYNC_PROJECT_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_ROOTS_MENU);
                        menuEntries.Add(SYNC_HIERARHY_MENU);
                        
                        if(selectedElement != null && 
                            selectedElement.Type == "Requirement" && 
                            selectedElement.Stereotype == "fmcreq" &&
                            !string.IsNullOrEmpty(_configuration.JiraURL) &&
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
                    else if(location == "MainMenu")
                    {
                        menuEntries.Add(EDIT_SETTINGS_MENU);
                    }
                    result = menuEntries.ToArray();
                break;
            }

            return result;
        }

        public void EA_GetMenuState(EAAPI.Repository repository, 
                                    string location, 
                                    string menuName, 
                                    string itemName, 
                                    ref bool isEnabled, 
                                    ref bool isChecked)
        {
            isEnabled = false;

            switch (itemName)
            {

                //case EXPORT_MENU:
                //    isEnabled = true;
                //    break;

                default:
                    isEnabled = true;
                    break;
            }


        }

        public void EA_MenuClick(EAAPI.Repository repository, string location, string menuName, string itemName)
        {
            if (_mainViewModel != null)
            {
                
                switch (itemName)
                {

                    //case EXPORT_MENU:
                    //    _mainViewModel.ExportToSpecIfCommand.Execute(null);
                    //    break;

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

            switch(itemName)
            {
                case EDIT_SETTINGS_MENU:
                    _mainViewModel.EditSettingsCommand.Execute(null);
                    break;
            }

        }

        public void EA_OnNotifyContextItemModified(EAAPI.Repository repository, string guid, EAAPI.ObjectType objectType)
        {
            ;
        }

        public bool EA_OnContextItemDoubleClicked(EAAPI.Repository repopsitory, string guid, EAAPI.ObjectType objectType)
        {
            bool result = false;

            if (objectType == EAAPI.ObjectType.otElement && _mainViewModel != null)
            {
                EAAPI.Element element = repopsitory.GetElementByGuid(guid);

                if (element.Type == "Requirement")
                {

                    _mainViewModel.OpenJiraViewCommand.Execute(guid);
                    result = true;
                }
            }

            return result;

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
