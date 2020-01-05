/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/hierarchies")]
    [ApiController]
    public class HierarchyController : Controller
    {
		ISpecIfDataReader _dataReader;

		ISpecIfDataWriter _dataWriter;

		public HierarchyController(ISpecIfDataReader dataReader, ISpecIfDataWriter dataWriter)
		{
			_dataReader = dataReader;
			_dataWriter = dataWriter;
		}

		/// <summary>
        /// Get all hierarchies.
        /// </summary>
        /// <returns></returns>
		[HttpGet]
        public List<Node> GetAllHierarchies()
        {
			return _dataReader.GetAllHierarchies();
        }

        /// <summary>
        /// Get hierarchy with a specific ID.
        /// </summary>
        /// <param name="id">The hierarchy ID.</param>
        /// <returns></returns>
		[HttpGet("{id}")]
        public Node GetHierarchyById(string id)
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

        /// <summary>
        /// Add a new hierarchy.
        /// </summary>
        /// <param name="hierarchy"></param>
        [HttpPost]
        public void AddNewHierarchy([FromBody]Node hierarchy)
        {
            _dataWriter.AddHierarchy(hierarchy);
        }
        
        /// <summary>
        /// Update an existing hierarchy.
        /// </summary>
        /// <param name="hierarchy"></param>
        [HttpPut]
        public void UpdateHierarchy([FromBody]Node hierarchy)
        {
            _dataWriter.SaveHierarchy(hierarchy);
        }
    }
}
