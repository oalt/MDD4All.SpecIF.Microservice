using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.SpecIF.Apps.ServiceStarter.Controllers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MDD4All.SpecIF.Apps.ServiceStarter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private List<ICommand> _commands = new List<ICommand>();

        private SettingsController _settingsController = new SettingsController();

        private Process _fileServiceProcess = null;

        private Process _eaServiceProcess = null;

        private Process _mongodbProcess = null;

        private Process _jiraProcess = null;

        private Process _integrationProcess = null;

        public MainViewModel() : base()
        {
            InitializeCommands();
        }

        

        

        private void InitializeCommands()
        {
            StartMongodbServiceCommand = new RelayCommand(ExecuteStartMongodbService, CanExecuteStartMongoDbService);
            _commands.Add(StartMongodbServiceCommand);

            StopMongodbServiceCommand = new RelayCommand(ExecuteStopMongoDbService, CanExecuteStopMongoDbService);
            _commands.Add(StopMongodbServiceCommand);

            StartFileServiceCommand = new RelayCommand(ExecuteStartFileService, CanExecuteStartFileService);
            _commands.Add(StartFileServiceCommand);

            StopFileServiceCommand = new RelayCommand(ExecuteStopFileService, CanExecuteStopFileService);
            _commands.Add(StopFileServiceCommand);

            StartEaServiceCommand = new RelayCommand(ExecuteStartEaService, CanStartEaService);
            _commands.Add(StartEaServiceCommand);

            StopEaServiceCommand = new RelayCommand(ExecuteStopEaServie, CanStopEaService);
            _commands.Add(StopEaServiceCommand);

            StartJiraServiceCommand = new RelayCommand(ExecuteStartJiraService, CanStartJiraService);
            _commands.Add(StartJiraServiceCommand);

            StopJiraServiceCommand = new RelayCommand(ExecuteStopJiraService, CanStopJiraService);
            _commands.Add(StopJiraServiceCommand);

            StartIntegrationServiceCommand = new RelayCommand(ExecuteStartIntegrationService, CanExecuteStartIntegrationsService);
            _commands.Add(StartIntegrationServiceCommand);

            StopIntegrationServiceCommand = new RelayCommand(ExecuteStopIntegrationService, CanStopIntegrationService);
            _commands.Add(StopIntegrationServiceCommand);

            SetExecutableCommand = new RelayCommand(ExecuteSetExecutablePath);
            _commands.Add(SetExecutableCommand);
        }




        private void StartService(ref Process process, string type, EventHandler eventHandler)
        {
            string arguments = ServiceExecutable + " " + type;

            FileInfo fileInfo = new FileInfo(ServiceExecutable);

            string workDirectory = fileInfo.Directory.FullName;

            process = ServiceProcessController.StartProcess("dotnet", arguments, workDirectory, ProcessWindowStyle.Normal, eventHandler);
        }

        private void MongodbProcessExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                RefreshCommands();
            }));
            RaisePropertyChanged("MongoServiceState"); 
        }

        private void FileProcessExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                RefreshCommands();
            }));
            RaisePropertyChanged("FileServiceState"); 
        }

        private void EaProcessExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                RefreshCommands();
            }));
            RaisePropertyChanged("EaServiceState"); 
        }

        private void JiraProcessExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                RefreshCommands();
            }));
            RaisePropertyChanged("JiraServiceState");
        }

        private void IntegrationProcessExit(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                RefreshCommands();
            }));
            RaisePropertyChanged("IntegrationServiceState");
        }

        #region Command_Implementations

        private void ExecuteSetExecutablePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            bool? dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != null && dialogResult == true)
            {
                ServiceExecutable = openFileDialog.FileName;
            }
        }

        private void ExecuteStopIntegrationService()
        {
            if(_integrationProcess != null && !_integrationProcess.HasExited)
            {
                _integrationProcess.Kill();
                _integrationProcess = null;

                RefreshCommands();
                RaisePropertyChanged("IntegrationServiceState");
            }
        }

        private bool CanStopIntegrationService()
        {
            return IntegrationServiceState; 
        }

        private void ExecuteStartIntegrationService()
        {
            StartService(ref _integrationProcess, "integration", IntegrationProcessExit);

            RaisePropertyChanged("IntegrationServiceState");
            RefreshCommands();
        }

        private bool CanExecuteStartIntegrationsService()
        {
            return !IntegrationServiceState;
        }

        private void ExecuteStopJiraService()
        {
            if (_jiraProcess != null && !_jiraProcess.HasExited)
            {
                _jiraProcess.Kill();
                _jiraProcess = null;

                RefreshCommands();
                RaisePropertyChanged("JiraServiceState");
            }
        }

        private bool CanStopJiraService()
        {
            return JiraServiceState;
        }

        private void ExecuteStartJiraService()
        {
            StartService(ref _jiraProcess, "jira", JiraProcessExit);

            RaisePropertyChanged("JiraServiceState");
            RefreshCommands();
        }

        private bool CanStartJiraService()
        {
            return !JiraServiceState;
        }



        private void ExecuteStopEaServie()
        {
            if (_eaServiceProcess != null && !_eaServiceProcess.HasExited)
            {
                _eaServiceProcess.Kill();
                _eaServiceProcess = null;

                RefreshCommands();
                RaisePropertyChanged("EaServiceState");
            }
        }

        private bool CanStopEaService()
        {
            return EaServiceState;
        }

        private void ExecuteStartEaService()
        {
            StartService(ref _eaServiceProcess, "ea", EaProcessExit);

            RaisePropertyChanged("EaServiceState");
            RefreshCommands();
        }

        private bool CanStartEaService()
        {
            return !EaServiceState;
        }



        private void ExecuteStartMongodbService()
        {
            StartService(ref _mongodbProcess, "mongodb", MongodbProcessExit);

            RaisePropertyChanged("MongoServiceState");
            RefreshCommands();
        }

        private bool CanExecuteStartMongoDbService()
        {
            return !MongoServiceState;
        }

        private void ExecuteStopMongoDbService()
        {
            if(_mongodbProcess != null && !_mongodbProcess.HasExited)
            {
                _mongodbProcess.Kill();
                _mongodbProcess = null;

                RefreshCommands();
                RaisePropertyChanged("MongoServiceState");
            }
        }

        private bool CanExecuteStopMongoDbService()
        {
            return MongoServiceState;
        }

        private void ExecuteStartFileService()
        {
            StartService(ref _fileServiceProcess, "file", FileProcessExit);

            RaisePropertyChanged("FileServiceState");
            RefreshCommands();
        }

        private bool CanExecuteStartFileService()
        {
            return !FileServiceState;
        }

        private void ExecuteStopFileService()
        {
            if (_fileServiceProcess != null && !_fileServiceProcess.HasExited)
            {
                _fileServiceProcess.Kill();
                _fileServiceProcess = null;

                RefreshCommands();
                RaisePropertyChanged("FileServiceState");
            }
        }

        private bool CanExecuteStopFileService()
        {
            return FileServiceState;
        }

       

        private void RefreshCommands()
        {
            foreach(ICommand command in _commands)
            {
                ((RelayCommand)command).RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Properties

        public bool MongoServiceState
        {
            get 
            {
                bool result = false;

                if(_mongodbProcess != null && !_mongodbProcess.HasExited)
                {
                    result = true;
                }

                return result; 
            }

            
        }

        

        public bool FileServiceState
        {
            get
            {
                bool result = false;

                if (_fileServiceProcess != null && !_fileServiceProcess.HasExited)
                {
                    result = true;
                }

                return result;
            }
        }

        

        public bool EaServiceState
        {
            get
            {
                bool result = false;

                if (_eaServiceProcess != null && !_eaServiceProcess.HasExited)
                {
                    result = true;
                }

                return result;
            }
        }

        

        public bool JiraServiceState
        {
            get
            {
                bool result = false;

                if (_jiraProcess != null && !_jiraProcess.HasExited)
                {
                    result = true;
                }

                return result;
            }
        }

       

        public bool IntegrationServiceState
        {
            get
            {
                bool result = false;

                if (_integrationProcess != null && !_integrationProcess.HasExited)
                {
                    result = true;
                }

                return result;
            }
        }

       

        public string ServiceExecutable
        {
            get { return _settingsController.Settings.SpecIfMicroservicePath; }
            set 
            { 
                _settingsController.Settings.SpecIfMicroservicePath = value;
                _settingsController.SaveSettings();
                RaisePropertyChanged("ServiceExecutable");
            }
        }



        #endregion

        #region Command_Definitions

        public ICommand StartMongodbServiceCommand { get; private set; }
        public ICommand StopMongodbServiceCommand { get; private set; }
        public ICommand StartFileServiceCommand { get; private set; }
        public ICommand StopFileServiceCommand { get; private set; }
        public ICommand StartEaServiceCommand { get; private set; }
        public ICommand StopEaServiceCommand { get; private set; }
        public ICommand StartJiraServiceCommand { get; private set; }
        public ICommand StopJiraServiceCommand { get; private set; }
        public ICommand StartIntegrationServiceCommand { get; private set; }
        public ICommand StopIntegrationServiceCommand { get; private set; }
        public ICommand SetExecutableCommand { get; private set; }
        #endregion

    }
}
