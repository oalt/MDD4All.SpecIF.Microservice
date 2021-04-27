using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.SpecIF.Apps.EaPlugin.Views;
using MDD4All.SpecIF.DataIntegrator.EA;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using EAAPI = EA;
using EADM = MDD4All.EAFacade.DataModels.Contracts;
using System.Collections.Generic;
using MDD4All.EAFacade.DataAccess.Cached;
using MDD4All.SpecIF.Apps.EaPlugin.Configuration;
using GalaSoft.MvvmLight.Ioc;
using MDD4All.Configuration.Contracts;
using MDD4All.Configuration.Views;
using NLog;
using MDD4All.SpecIF.DataProvider.Jira;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private EAAPI.Repository _repository;

        private ISpecIfMetadataReader _metadataReader;
        private ISpecIfDataReader _specIfDataReader;
        private ISpecIfDataWriter _requirementMasterDataWriter;

        private ProjectIntegrator _projectIntegrator;

        IConfigurationReaderWriter<SpecIfPluginConfiguration> _configurationReaderWriter;

        public MainViewModel(EAAPI.Repository repository)
        {
            _repository = repository;



            ExportToSpecIfCommand = new RelayCommand(ExecuteExportToSpecIfCommand);
            SynchronizeProjectRootsCommand = new RelayCommand(ExecuteSynchronizeProjectRoots);
            SynchronizeProjectHierarchyRootsCommand = new RelayCommand(ExecuteSynchronizeProjectHierarchyRoots);
            SynchronizeHierarchyResourcesCommand = new RelayCommand(ExecuteSynchronizeHierarchyResources);
            NewRequirementCreationRequestedCommand = new RelayCommand(ExecuteNewRequirementCreationRequested);
            AddSingleRequirementToSpecIfCommand = new RelayCommand(ExecuteAddSingleRequirementToSpecIF);
            AddSpecificationToSpecIfCommand = new RelayCommand(ExecuteAddSpecificationToSpecIF);
            EditSettingsCommand = new RelayCommand(ExecuteEditSettings);
            OpenJiraViewCommand = new RelayCommand<string>(ExecuteOpenJiraView);
            SynchonizeSingleElementCommand = new RelayCommand(ExecuteSynchronizeSingleElement);
            DisplayVersionCommand = new RelayCommand(ExecuteDisplayVersion);

            _configurationReaderWriter = SimpleIoc.Default.GetInstance<IConfigurationReaderWriter<SpecIfPluginConfiguration>>();

            InitializeDataProviders();
        }

        

        private void InitializeDataProviders()
        {
            SpecIfPluginConfiguration configuration = _configurationReaderWriter.GetConfiguration();

            _metadataReader = new SpecIfFileMetadataReader(configuration.SpecIfMetadataDirectory);

            _specIfDataReader = new SpecIfJiraDataReader(configuration.JiraURL,
                                                         configuration.JiraUserName,
                                                         configuration.JiraApiKey,
                                                         _metadataReader);

            _requirementMasterDataWriter = new SpecIfJiraDataWriter(configuration.JiraURL,
                                                                    configuration.JiraUserName,
                                                                    configuration.JiraApiKey,
                                                                    _metadataReader,
                                                                    _specIfDataReader);

            _projectIntegrator = new ProjectIntegrator(_repository,
                                                       _metadataReader,
                                                       _specIfDataReader);
        }

        public string Version
        {
            get
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;

                return version;
            }
        }

        public ICommand ExportToSpecIfCommand { get; private set; }
        public ICommand SynchronizeProjectRootsCommand { get; private set; }
        public ICommand SynchronizeProjectHierarchyRootsCommand { get; private set; }
        public ICommand SynchronizeHierarchyResourcesCommand { get; private set; }
        public ICommand NewRequirementCreationRequestedCommand { get; private set; }
        public ICommand AddSingleRequirementToSpecIfCommand { get; private set; }
        public ICommand AddSpecificationToSpecIfCommand { get; private set; }
        public ICommand EditSettingsCommand { get; private set; }
        public ICommand OpenJiraViewCommand { get; private set; }
        public ICommand SynchonizeSingleElementCommand { get; private set; }
        public ICommand DisplayVersionCommand { get; private set; }

        private void ExecuteDisplayVersion()
        {
            MessageBox.Show("SpecIF Plugin\r\n© MDD4All.de\r\nVersion: " + Version, "About...", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExecuteExportToSpecIfCommand()
        {
            if(_repository != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "SpecIF files|*.specif";

                DialogResult dialogResult = saveFileDialog.ShowDialog();

                if (dialogResult == DialogResult.OK)
                {
                    try
                    {
                        EAAPI.Package selectedPackage = _repository.GetTreeSelectedPackage();

                        DataModels.SpecIF specIF=null;

                        //EaUmlToSpecIfConverter converter;

                        //converter = new EaUmlToSpecIfConverter(_repository);

                        //specIF = converter.ConvertModelToSpecIF(selectedPackage);

                        SpecIfFileReaderWriter.SaveSpecIfToFile(specIF, saveFileDialog.FileName);

                        MessageBox.Show("SpecIF export successfully finished!", "Information",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch(Exception exception)
                    {
                        MessageBox.Show(exception.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ExecuteSynchronizeProjectRoots()
        {
            logger.Info("Starting synchonization of Project roots...");
            SynchronizeProjectRootsAsync();
        }

        private async void SynchronizeProjectRootsAsync()
        {
            await Task.Run(() =>
            {
                _projectIntegrator.SynchronizeProjectRoots();
            });
            logger.Info("Synchonization finished.");
        }

        private void ExecuteSynchronizeProjectHierarchyRoots()
        {
            logger.Info("Starting synchonization of Hierarchy roots...");
            SynchronizeProjectHierarchyRootsAsync();
        }

        private async void SynchronizeProjectHierarchyRootsAsync()
        {
            await Task.Run(() =>
            {
                _projectIntegrator.SynchronizeProjectHierarchyRoots();
            });
            logger.Info("Synchonization finished.");
        }

        private void ExecuteSynchronizeHierarchyResources()
        {
            logger.Info("Starting synchonization...");
            SynchronizeHierarchiyResourcesAsync();
        }

        private async void SynchronizeHierarchiyResourcesAsync()
        {
            await Task.Run(() =>
            {
                _projectIntegrator.SynchronizeHierarchyResources();
                
            });
            logger.Info("Synchonization finished.");
        }

        private void ExecuteNewRequirementCreationRequested()
        {
            ProjectDescriptor project = ExecuteSelectProject();

            if(project != null)
            {

            }
        }

        private void ExecuteSynchronizeSingleElement()
        {
            object selectedItem;

            EAAPI.ObjectType objectType = _repository.GetTreeSelectedItem(out selectedItem);

            EAAPI.Element element = selectedItem as EAAPI.Element;

            _projectIntegrator.SynchronizeSingleElement(element);
        }


        private void ExecuteAddSingleRequirementToSpecIF()
        {
            object selectedItem;

            EAAPI.ObjectType objectType = _repository.GetTreeSelectedItem(out selectedItem);

            EAAPI.Element element = selectedItem as EAAPI.Element;

            ProjectDescriptor project = ExecuteSelectProject();

            if (project != null)
            {
                logger.Info("Integrating requirement " + element.Name + "...");
                Resource repositoryResource = _projectIntegrator.AddRequirementToSpecIF(_requirementMasterDataWriter, element, project.ID);

                if(repositoryResource == null)
                {
                    logger.Error("Error integrating requirement.");
                }
                else
                {
                    logger.Info("Finished.");
                }

            }

            
          
        }

        private void ExecuteAddSpecificationToSpecIF()
        {
            logger.Info("Starting integration...");
            ExecuteAddSpecificationToSpecIfAsync();
        }


        private async void ExecuteAddSpecificationToSpecIfAsync()
        {
            EAAPI.Package treeSelectedPackage = _repository.GetTreeSelectedPackage();

            if(treeSelectedPackage.Element.Stereotype == "specification")
            {
                CachedDataProvider eaCacheDataProvider = new CachedDataProvider(_repository);

                EADM.Element cachedSpecification = eaCacheDataProvider.GetCachedSpecification(treeSelectedPackage);

                List<EADM.Element> linearElementList = eaCacheDataProvider.GetSpecificationElementList(cachedSpecification);

                List<EADM.Element> cachedRequirements = linearElementList.FindAll(el => el.Type == "Requirement" && 
                                                                                  !el.TaggedValues.Exists(tv => tv.Name == "specifId"));

                if(cachedRequirements.Count > 0)
                {
                    ProjectDescriptor project = ExecuteSelectProject();

                    if(project != null)
                    {
                        await Task.Run(() =>
                        {
                            bool error = false;

                            foreach (EADM.Element cachedElement in cachedRequirements)
                            {
                                EAAPI.Element element = _repository.GetElementByID(cachedElement.ElementID);

                                if (element != null)
                                {
                                    logger.Info("Integrating requirement " + cachedElement.Name + "...");
                                    Resource repositoryResource = _projectIntegrator.AddRequirementToSpecIF(_requirementMasterDataWriter, element, project.ID);
                                    if (repositoryResource == null)
                                    {
                                        logger.Error("Error integrating requirement. Abort.");
                                        error = true;
                                        break;
                                    }
                                }
                            }

                            if (!error)
                            {
                                logger.Info("Integration finished.");
                            }
                        });
                    }
                }

            }
        }

        private void ExecuteEditSettings()
        {
            if (_configurationReaderWriter != null)
            {
                PropertyDialog propertyDialog = new PropertyDialog();

                SpecIfPluginConfiguration configuration = _configurationReaderWriter.GetConfiguration();

                propertyDialog.SetPropertyData(configuration);

                propertyDialog.ShowDialog();

                _configurationReaderWriter.StoreConfiguration(configuration);

                InitializeDataProviders();
            }

        }

        private ProjectDescriptor ExecuteSelectProject()
        {
            ProjectDescriptor result = null;

            ProjectSelectionWindow projectSelectionWindow = new ProjectSelectionWindow();

            ProjectSelectionViewModel projectSelectionViewModel = new ProjectSelectionViewModel(_specIfDataReader, projectSelectionWindow);

            projectSelectionWindow.DataContext = projectSelectionViewModel;

            bool? dialogResult = projectSelectionWindow.ShowDialog();

            if (dialogResult != null && dialogResult == true)
            {
                if (projectSelectionViewModel.SelectedProject != null)
                {
                    result = projectSelectionViewModel.SelectedProject.ProjectDescriptor;
                }
            }

            return result;
        }

        private void ExecuteOpenJiraView(string identifier)
        {
            if(_configurationReaderWriter != null)
            {
                string jiraURL = _configurationReaderWriter.GetConfiguration().JiraURL;
                System.Diagnostics.Process.Start(jiraURL + "/browse/" + identifier);
            }   
        }
    }
}
