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
    [Route("SpecIF/Hierarchy")]
    public class HierarchyController : Controller
    {
		ISpecIfDataReader _dataReader;

		ISpecIfDataWriter _dataWriter;

		public HierarchyController(ISpecIfDataReader dataReader, ISpecIfDataWriter dataWriter)
		{
			_dataReader = dataReader;
			_dataWriter = dataWriter;
		}

		// GET: SpecIF/Hierarchy
		[HttpGet]
        public List<Node> Get()
        {
			return _dataReader.GetAllHierarchies();
        }

		[HttpGet("{id}")]
		public Node Get(string id)
		{
			Node result = null;
			if (!string.IsNullOrEmpty(id) && _dataReader != null)
			{
				Node hierarchy = _dataReader.GetHierarchyByKey(new Key(id));

				if (hierarchy != null)
				{
					result = hierarchy;
				}
			}

			return result;
		}

        [HttpPost]
        public void Post([FromBody]Node hierarchy)
        {
			_dataWriter.AddHierarchy(hierarchy);
        }
        
        [HttpPut]
		public void Put([FromBody]Node hierarchy)
		{
			_dataWriter.UpdateHierarchy(hierarchy);
		}
    }
}
