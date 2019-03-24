/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MDD4All.SpecIF.DataModels.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [Route("SpecIF/ServiceDescription")]
    public class ServiceDescriptionController : Controller
    {
		private ISpecIfServiceDescription _serviceDescription;

		public ServiceDescriptionController(ISpecIfServiceDescription serviceDescription)
		{
			_serviceDescription = serviceDescription;
		}

		[HttpGet]
		public SpecIfServiceDescription Get()
		{
			return _serviceDescription as SpecIfServiceDescription;
		}
	}
}