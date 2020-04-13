using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MDD4All.SpecIF.ServiceDataProvider;
using MDD4All.SpecIF.ViewModels.IntegrationService;

namespace MDD4All.SpecIf.Microservice.Controllers
{
   
    //[Route("services")]
    public class ServiceController : Controller
	{
		private ISpecIfServiceDescriptionProvider _specIfServiceDescriptionProvider;

		public ServiceController(ISpecIfServiceDescriptionProvider specIfServiceDescriptionProvider)
		{
			_specIfServiceDescriptionProvider = specIfServiceDescriptionProvider;
		}

		public IActionResult Index()
		{
			IntegrationServiceViewModel viewModel = new IntegrationServiceViewModel(_specIfServiceDescriptionProvider);

			return View(viewModel);
		}

		[ActionName("Refresh")]
		public IActionResult RefreshServiceList()
		{
			IntegrationServiceViewModel viewModel = new IntegrationServiceViewModel(_specIfServiceDescriptionProvider);

			viewModel.RefreshServiceDescriptionsCommand.Execute(null);

			return RedirectToAction("Index", "Service");
		}

		

		//public IActionResult Error()
		//{
		//	return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		//}
	}
}
