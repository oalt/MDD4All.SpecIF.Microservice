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
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("specif/v{version:apiVersion}/data-types")]
    [ApiController]
    public class DataTypeController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

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
        /// Returns the latest revision of the data type with the given ID. 
        /// </summary>
        /// <param name="id">The data type ID.</param>
        /// <returns>The data type or a not found code.</returns>
		[HttpGet("{id}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<DataType> GetDataTypeById(string id)
		{
            ActionResult<DataType> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                DataType dbResult = _metadataReader.GetDataTypeByKey(new Key(id));

                if (dbResult != null)
                {
                    result = dbResult;
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
        /// Returns the data type with the specific revision.
        /// </summary>
        /// <param name="id">The data type ID.</param>
        /// <param name="revision">The data type revision.</param>
        /// <returns></returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(DataType), 200)]
        [ProducesResponseType(404)]
        public ActionResult<DataType> GetDataTypeRevision(string id, string revision)
        {
            ActionResult<DataType> result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                DataType statementClass = _metadataReader.GetDataTypeByKey(new Key() { ID = id, Revision = revisionObject });
                if (statementClass != null)
                {
                    result = new ObjectResult(statementClass);
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }
    }
}