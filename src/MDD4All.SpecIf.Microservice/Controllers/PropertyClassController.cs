/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [Route("SpecIF/PropertyClass")]
    public class PropertyClassController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public PropertyClassController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		[HttpGet]
		public List<PropertyClass> Get()
		{
			return _metadataReader.GetAllPropertyClasses();
		}
	}
}