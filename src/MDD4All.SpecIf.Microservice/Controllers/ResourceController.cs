/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Mvc;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [Route("SpecIF/Resource")]
    public class ResourceController : Controller
    {
		private ISpecIfDataReader _specIfDataReader;
		private ISpecIfDataWriter _specIfDataWriter;

		public ResourceController(ISpecIfDataReader specIfDataReader,
								  ISpecIfDataWriter specIfDataWriter)
		{
			_specIfDataReader = specIfDataReader;
			_specIfDataWriter = specIfDataWriter;
		}

		[HttpGet("{id}")]
		public IActionResult Get(string id)
		{
			return Get(id, Key.LATEST_REVISION);
		}

		[HttpGet("{id}/{revision}")]
		public IActionResult Get(string id, string revision)
		{
			IActionResult result = NotFound();

			if (!string.IsNullOrEmpty(id))
			{
				Resource resource = _specIfDataReader.GetResourceByKey(new Key() { ID = id, Revision = revision });
				if (resource != null)
				{
					result = new ObjectResult(resource);
				}
			}

			return result;
		}

		[HttpGet("LatestRevision/{id}")]
		public string GetLatestRevision(string id)
		{
			return _specIfDataReader.GetLatestResourceRevision(id);
		}

		[HttpPost]
        public void Post([FromBody]Resource resource)
        {
			_specIfDataWriter.AddResource(resource);
        }

		[HttpPut]
		public void Put([FromBody]Resource resource)
		{
			_specIfDataWriter.UpdateResource(resource);
		}

	}
}
