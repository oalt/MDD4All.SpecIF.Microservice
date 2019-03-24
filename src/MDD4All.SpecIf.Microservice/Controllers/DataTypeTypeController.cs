/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [Route("SpecIF/DataTypeType")]
    public class DataTypeTypeController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public DataTypeTypeController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		[HttpGet]
		public List<string> Get()
		{
			return _metadataReader.GetDataTypeTypes();
		}

	}
}