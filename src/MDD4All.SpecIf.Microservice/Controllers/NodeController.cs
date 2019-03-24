/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [Route("SpecIF/Node")]
    public class NodeController : Controller
    {
		private ISpecIfDataWriter _specIfDataWriter;

		public NodeController(ISpecIfDataWriter dataWriter)
		{
			_specIfDataWriter = dataWriter;
		}

		[HttpPost]
		public void Post([FromBody]Node node)
		{
			_specIfDataWriter.AddNode(node);
		}

		[HttpPut]
		public void Put([FromBody]Node node)
		{
			_specIfDataWriter.UpdateNode(node);
		}
	}
}