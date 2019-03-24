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
    [Route("SpecIF/EnumOptions")]
    public class EnumOptionsController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public EnumOptionsController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		[HttpGet("{dataTypeId}")]
		public List<EnumValue> Get(string dataTypeId)
		{
			return _metadataReader.GetEnumOptions(dataTypeId);
		}

	}
}