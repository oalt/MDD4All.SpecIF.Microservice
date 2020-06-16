using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.SpecIF.DataIntegrator.EA;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.EA.Converters;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Windows.Forms;
using System.Windows.Input;
using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private EAAPI.Repository _repository;

        private ISpecIfMetadataReader _metadataReader;

        private ProjectIntegrator _projectIntegrator;

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
        }

        public ICommand ExportToSpecIfCommand { get; private set; }
        public ICommand SynchronizeProjectRootsCommand { get; private set; }
        public ICommand SynchronizeProjectHierarchyRootsCommand { get; private set; }
        public ICommand SynchronizeHierarchyResourcesCommand { get; private set; }

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



    }
}
