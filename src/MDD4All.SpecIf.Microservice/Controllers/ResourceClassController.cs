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
    [Route("SpecIF/ResourceClass")]
    public class ResourceClassController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public ResourceClassController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		[HttpGet]
		public List<ResourceClass> Get()
		{
			return _metadataReader.GetAllResourceClasses();
		}

		[HttpGet("{id}")]
		public ResourceClass Get(string id)
		{
			return _metadataReader.GetResourceClassByKey(new Key(id));
		}
	}
}