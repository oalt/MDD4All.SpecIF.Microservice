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

        private SpecIfPluginConfiguration _configuration;

        public MainViewModel(EAAPI.Repository repository,
                             ISpecIfMetadataReader metadataReader,
                             ISpecIfDataReader specIfDataReader,
                             ISpecIfDataWriter requirementMasterDataWriter)
        {
            _repository = repository;
            _metadataReader = metadataReader;
            _specIfDataReader = specIfDataReader;
            _requirementMasterDataWriter = requirementMasterDataWriter;

            _repository.CreateOutputTab("SpecIF");

            _repository.EnsureOutputVisible("SpecIF");

            _projectIntegrator = new ProjectIntegrator(_repository, 
                                                       _metadataReader,
                                                       _specIfDataReader);

            ExportToSpecIfCommand = new RelayCommand(ExecuteExportToSpecIfCommand);
            SynchronizeProjectRootsCommand = new RelayCommand(ExecuteSynchronizeProjectRoots);
            SynchronizeProjectHierarchyRootsCommand = new RelayCommand(ExecuteSynchronizeProjectHierarchyRoots);
            SynchronizeHierarchyResourcesCommand = new RelayCommand(ExecuteSynchronizeHierarchyResources);
            NewRequirementCreationRequestedCommand = new RelayCommand(ExecuteNewRequirementCreationRequested);
            AddSingleRequirementToSpecIfCommand = new RelayCommand(ExecuteAddSingleRequirementToSpecIF);
            AddSpecificationToSpecIfCommand = new RelayCommand(ExecuteAddSpecificationToSpecIF);
            EditSettingsCommand = new RelayCommand(ExecuteEditSettings);
            OpenJiraViewCommand = new RelayCommand<string>(ExecuteOpenJiraView);

            IConfigurationReaderWriter<SpecIfPluginConfiguration> configurationReaderWriter = SimpleIoc.Default.GetInstance<IConfigurationReaderWriter<SpecIfPluginConfiguration>>();
            
            if(configurationReaderWriter != null)
            {
                _configuration = configurationReaderWriter.GetConfiguration();
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
            _projectIntegrator.SynchronizeProjectRoots();
        }

        private void ExecuteSynchronizeProjectHierarchyRoots()
        {
            _projectIntegrator.SynchronizeProjectHierarchyRoots();
        }

        private void ExecuteSynchronizeHierarchyResources()
        {
            logger.Info("Starting synchonization...");
            _projectIntegrator.SynchronizeHierarchyResources();
            logger.Info("Synchonization finished.");
        }

        private void ExecuteNewRequirementCreationRequested()
        {
            ProjectDescriptor project = ExecuteSelectProject();

            if(project != null)
            {

            }
        }


        private void ExecuteAddSingleRequirementToSpecIF()
        {
            object selectedItem;

            EAAPI.ObjectType objectType = _repository.GetTreeSelectedItem(out selectedItem);

            EAAPI.Element element = selectedItem as EAAPI.Element;

            ProjectDescriptor project = ExecuteSelectProject();

            if (project != null)
            {
                _repository.WriteOutput("SpecIF", "Integrating requirement " + element.Name + "...", element.ElementID);
                Resource repositoryResource = _projectIntegrator.AddRequirementToSpecIF(_requirementMasterDataWriter, element, project.ID);

                if(repositoryResource == null)
                {
                    _repository.WriteOutput("SpecIF", "Error integrating requirement.", element.ElementID);
                }
                else
                {
                    _repository.WriteOutput("SpecIF", "Finished.", 0);
                }

            }

            
          
        }

        private void ExecuteAddSpecificationToSpecIF()
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
                        bool error = false;

                        foreach (EADM.Element cachedElement in cachedRequirements)
                        {
                            EAAPI.Element element = _repository.GetElementByID(cachedElement.ElementID);

                            if(element != null)
                            {
                                _repository.WriteOutput("SpecIF", "Integrating requirement " + cachedElement.Name + "...", cachedElement.ElementID);
                                Resource repositoryResource = _projectIntegrator.AddRequirementToSpecIF(_requirementMasterDataWriter, element, project.ID);
                                if(repositoryResource == null)
                                {
                                    _repository.WriteOutput("SpecIF", "Error integrating requirement. Abort.", cachedElement.ElementID);
                                    error = true;
                                    break;
                                }
                            }
                        }

                        if(!error)
                        {
                            _repository.WriteOutput("SpecIF", "Integration finished.", 0);
                        }
                    }
                }

            }
        }

        private void ExecuteEditSettings()
        {
            PropertyDialog propertyDialog = new PropertyDialog();

            propertyDialog.SetPropertyData(_configuration);

            propertyDialog.ShowDialog();

            IConfigurationReaderWriter<SpecIfPluginConfiguration> configurationReaderWriter = SimpleIoc.Default.GetInstance<IConfigurationReaderWriter<SpecIfPluginConfiguration>>();

            if (configurationReaderWriter != null)
            {
                configurationReaderWriter.StoreConfiguration(_configuration);
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

        private void ExecuteOpenJiraView(string url)
        {
            //JiraIssueBrowswerWindow jiraIssueBrowswerWindow = new JiraIssueBrowswerWindow();
            //jiraIssueBrowswerWindow.ShowDialog();
            System.Diagnostics.Process.Start("https://karlmayer.atlassian.net/browse/WIN-1");
        }
    }
}
