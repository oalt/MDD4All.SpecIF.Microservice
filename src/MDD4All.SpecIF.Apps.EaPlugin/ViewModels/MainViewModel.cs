using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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

        public MainViewModel(EAAPI.Repository repository)
        {
            _repository = repository;

            ExportToSpecIfCommand = new RelayCommand(ExecuteExportToSpecIfCommand);
        }

        public ICommand ExportToSpecIfCommand { get; private set; }

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

    }
}
