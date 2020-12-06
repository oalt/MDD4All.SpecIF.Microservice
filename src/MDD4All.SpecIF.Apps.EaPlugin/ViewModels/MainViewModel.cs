using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.Apps.EaPlugin.Views;
using MDD4All.SpecIF.DataIntegrator.EA;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.File;
using MDD4All.SpecIF.DataModels.Manipulation;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using EAAPI = EA;
using EADM = MDD4All.EAFacade.DataModels.Contracts;
using System.Collections.Generic;
using MDD4All.EAFacade.DataAccess.Cached;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private EAAPI.Repository _repository;

        private ISpecIfMetadataReader _metadataReader;

        private ISpecIfDataReader _dataReader;

        private ProjectIntegrator _projectIntegrator;

        public ObservableCollection<ProjectDescriptor> Projects { get; set; } = new ObservableCollection<ProjectDescriptor>();

        public MainViewModel(EAAPI.Repository repository,
                             ISpecIfMetadataReader metadataReader)
        {
            _repository = repository;
            _metadataReader = metadataReader;

            _projectIntegrator = new ProjectIntegrator(_repository, _metadataReader);

            ExportToSpecIfCommand = new RelayCommand(ExecuteExportToSpecIfCommand);
            SynchronizeProjectRootsCommand = new RelayCommand(ExecuteSynchronizeProjectRoots);
            SynchronizeProjectHierarchyRootsCommand = new RelayCommand(ExecuteSynchronizeProjectHierarchyRoots);
            SynchronizeHierarchyResourcesCommand = new RelayCommand(ExecuteSynchronizeHierarchyResources);
            NewRequirementCreationRequestedCommand = new RelayCommand(ExecuteNewRequirementCreationRequested);
            AddSingleRequirementToSpecIfCommand = new RelayCommand(ExecuteAddSingleRequirementToSpecIF);
            AddSpecificationToSpecIfCommand = new RelayCommand(ExecuteAddSpecificationToSpecIF);
        }

        

        public string GetSpecIfRepositoryURL()
        {
            string result = null;

            EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecIfIntegrationPackage", "");

            EAAPI.Element packageElement;

            if (searchResult.Count > 0)
            {
                try
                {
                    packageElement = searchResult.GetAt(0) as EAAPI.Element;

                    result = packageElement.GetTaggedValueString("specifApiUrl");
                }
                catch
                {

                }
            }

            return result;
        }


        public ICommand ExportToSpecIfCommand { get; private set; }
        public ICommand SynchronizeProjectRootsCommand { get; private set; }
        public ICommand SynchronizeProjectHierarchyRootsCommand { get; private set; }
        public ICommand SynchronizeHierarchyResourcesCommand { get; private set; }
        public ICommand NewRequirementCreationRequestedCommand { get; private set; }
        public ICommand AddSingleRequirementToSpecIfCommand { get; private set; }
        public ICommand AddSpecificationToSpecIfCommand { get; private set; }

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
            _projectIntegrator.SynchronizeHierarchyResources();
        }

        private void ExecuteNewRequirementCreationRequested()
        {
            ProjectSelectionViewModel projectSelectionViewModel = new ProjectSelectionViewModel();

            projectSelectionViewModel.Projects = Projects;

            ProjectSelectionWindow projectSelectionWindow = new ProjectSelectionWindow();
            projectSelectionWindow.DataContext = projectSelectionViewModel;

            bool? dialogResult = projectSelectionWindow.ShowDialog();

            if(dialogResult != null && dialogResult == true)
            {
                if(projectSelectionViewModel.SelectedProject != null)
                {

                }
            }
        }


        private void ExecuteAddSingleRequirementToSpecIF()
        {
            object selectedItem;

            EAAPI.ObjectType objectType = _repository.GetTreeSelectedItem(out selectedItem);

            EAAPI.Element element = selectedItem as EAAPI.Element;

            string apiUrl = GetSpecIfRepositoryURL();

            // TODO project id

            // TODO login data to config file

            LoginData loginData = new LoginData();

            loginData.UserName = "olli";
            loginData.Password = "password";

            Resource repositoryResource = _projectIntegrator.AddRequirementToSpecIF(apiUrl, loginData, element, "_d383255183995289153c412de3b5bffda9bcf45b_10000");
          
        }

        private void ExecuteAddSpecificationToSpecIF()
        {
            EAAPI.Package treeSelectedPackage = _repository.GetTreeSelectedPackage();

            if(treeSelectedPackage.Element.Stereotype == "specification")
            {
                CachedDataProvider eaCacheDataProvider = new CachedDataProvider(_repository);

                EADM.Element cachedSpecification = eaCacheDataProvider.GetCachedSpecification(treeSelectedPackage);

                List<EADM.Element> linearElementList = eaCacheDataProvider.GetSpecificationElementList(cachedSpecification);

                List<EADM.Element> cachedRequirements = linearElementList.FindAll(el => el.Type == "Requirement" && !el.TaggedValues.Exists(tv => tv.Name == "specifId"));

                // TODO Add data to SpecIF

            }
        }

    }
}
