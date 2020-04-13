using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.ServiceDataProvider;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MDD4All.SpecIF.ViewModels.IntegrationService
{
     
	public class IntegrationServiceViewModel : ViewModelBase
	{
		private ISpecIfServiceDescriptionProvider _dataProvider;

		public ObservableCollection<SpecIfServiceDescription> ServiceDescriptions { get; set; }

		public IntegrationServiceViewModel(ISpecIfServiceDescriptionProvider dataProvider)
		{
			_dataProvider = dataProvider;

			RefreshServiceDescriptionsCommand = new RelayCommand(ExecuteRefreshServiceCommand);

			List<SpecIfServiceDescription> descriptions = _dataProvider.GetAvailableServices();

			ServiceDescriptions = new ObservableCollection<SpecIfServiceDescription>(descriptions);
		}

		

		public ICommand RefreshServiceDescriptionsCommand { get; private set; }

		private void ExecuteRefreshServiceCommand()
		{
			_dataProvider.Refresh();
		}

	}
}
