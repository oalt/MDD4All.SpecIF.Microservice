using GalaSoft.MvvmLight;
using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class ProjectSelectionViewModel : ViewModelBase
    {

        public ProjectSelectionViewModel()
        {

        }

        public ObservableCollection<ProjectDescriptor> Projects { get; set; } = new ObservableCollection<ProjectDescriptor>();

        public ProjectDescriptor SelectedProject { get; set; } = null;

    }
}
