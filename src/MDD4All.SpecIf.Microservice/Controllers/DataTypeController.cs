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
    [Route("SpecIF/DataType")]
    public class DataTypeController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public DataTypeController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		[HttpGet]
		public IEnumerable<DataType> Get()
		{
			return _metadataReader.GetAllDataTypes();
		}

		[HttpGet("{id}")]
		public DataType Get(string id)
		{
			return _metadataReader.GetDataTypeById(id);
		}
	}
}