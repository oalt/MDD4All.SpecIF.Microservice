/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    /// <summary>
    /// The controller to manage Data Types.
    /// </summary>
    [ApiVersion("1.1")]
    [Produces("application/json")]
    [Route("specif/v{version:apiVersion}/dataTypes")]
    [ApiController]
    public class DataTypeController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metadataReader">The metadata reader.</param>
		public DataTypeController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

        /// <summary>
        /// Returns all data types with all available revisions.
        /// </summary>
        /// <returns>A list of all available data types.</returns>
        /// <response code="200">List of data types suceessfull returned.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<DataType>), 200)]
        public ActionResult<List<DataType>> GetAllDataTypes()
		{

            ActionResult<List<DataType>> result = NotFound();

            List<DataType> dbResult = _metadataReader.GetAllDataTypes();

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
            
		}

        /// <summary>
        /// Returns a data type with the given ID. 
        /// </summary>
        /// <param name="id">The data type ID.</param>
        /// <param name="revision">The data type revision id.</param>
        /// <returns>The data type or a not found code.</returns>
		[HttpGet("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<DataType> GetDataTypeById(string id, [FromQuery]string revision)
		{
            ActionResult<DataType> result = NotFound();

            if (!string.IsNullOrEmpty(id) )
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string rev = revision.Replace("%2F", "/");
                    
                    DataType statementClass = _metadataReader.GetDataTypeByKey(new Key() { ID = id, Revision = rev });
                    if (statementClass != null)
                    {
                        result = new ObjectResult(statementClass);
                    }
                }
                else
                {
                    result = NotFound();
                }

            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
		}

        /// <summary>
        /// Returns all data type revisions for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<DataType>), 200)]
        public ActionResult<List<DataType>> GetAllDatatypeRevisions(string id)
        {
            ActionResult<List<DataType>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<DataType> dbResult = _metadataReader.GetAllDataTypeRevisions(id);
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Create a data type.
        /// </summary>
        /// <param name="dataType">The data type to create.</param>
        /// <returns>The updated data type element.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public ActionResult CreateDataType([FromBody]DataType dataType)
        {
            ActionResult result = NotFound();

            return result;
        }

        /// <summary>
        /// Update the data type; the supplied ID must exist.
        /// </summary>
        /// <param name="dataType">The data type data.</param>
        /// <returns>The updated data type element.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public ActionResult UpdateDataType([FromBody]DataType dataType)
        {
            ActionResult result = NotFound();

            return result;
        }


        /// <summary>
        /// Delete the data type; the supplied ID must exist. 
        /// Return an error if there are depending model elements. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <param name="mode">Delete mode. ?mode=forced results in deleting all directly and indirectly depending model elements.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteDataType(string id, [FromQuery]string revision, [FromQuery]string mode)
        {
            ActionResult result = NotFound();

            return result;
        }
       
    }
}