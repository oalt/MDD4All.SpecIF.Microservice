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
    [Route("SpecIF/StatementClass")]
    public class StatementClassController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

		public StatementClassController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		//[HttpGet]
		//public List<StatementClass> Get()
		//{
		//	return _metadataReader.GetAllResourceClasses();
		//}

		[HttpGet("{id}")]
		public StatementClass Get(string id)
		{
			return _metadataReader.GetStatementClassByKey(new Key(id));
		}
	}
}