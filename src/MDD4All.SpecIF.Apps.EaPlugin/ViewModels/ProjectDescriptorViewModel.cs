using GalaSoft.MvvmLight;
using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDD4All.SpecIF.DataModels.Manipulation;

namespace MDD4All.SpecIF.Apps.EaPlugin.ViewModels
{
    public class ProjectDescriptorViewModel : ViewModelBase
    {

       

        public ProjectDescriptorViewModel(ProjectDescriptor projectDescriptor)
        {
            ProjectDescriptor = projectDescriptor;
        }

        public ProjectDescriptor ProjectDescriptor { get; set; }

        public string Title 
        {
            get
            {
                string result = "???";

                if (ProjectDescriptor != null)
                {
                    result = ProjectDescriptor.GetTitleValue();
                }

                return result;
            }
        }

    }
}
