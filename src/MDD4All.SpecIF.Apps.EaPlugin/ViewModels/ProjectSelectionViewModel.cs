using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class ProjectSelectionViewModel : ViewModelBase
    {
        private ISpecIfDataReader _specIfDataReader;

        private Window _dialogReference;

        public ProjectSelectionViewModel(ISpecIfDataReader specIfDataReader,
                                         Window dialogReference = null)
        {
            _specIfDataReader = specIfDataReader;
            _dialogReference = dialogReference;

            OkCommand = new RelayCommand(ExecuteOkCommand);
            CancelCommand = new RelayCommand(ExecuteCancelCommand);

            InitializeProjectList();
        }


        private void InitializeProjectList()
        {
            List<ProjectDescriptor> projectDescriptors = _specIfDataReader.GetProjectDescriptions();

            foreach(ProjectDescriptor project in projectDescriptors)
            {
                Projects.Add(new ProjectDescriptorViewModel(project));
            }

            if(Projects.Count > 0)
            {
                SelectedProject = Projects[0];
            }

        }
        

        public ObservableCollection<ProjectDescriptorViewModel> Projects { get; set; } = new ObservableCollection<ProjectDescriptorViewModel>();

        public ProjectDescriptorViewModel SelectedProject { get; set; } = null;

        public ICommand OkCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        private void ExecuteOkCommand()
        {
            if(_dialogReference != null)
            {
                _dialogReference.DialogResult = true;
            }
            CloseDialog();
        }

        private void ExecuteCancelCommand()
        {
            SelectedProject = null;
            CloseDialog();
        }

        private void CloseDialog()
        {
            if(_dialogReference != null && _dialogReference.IsVisible)
            {
                _dialogReference.Close();
            }

        }

    }
}
