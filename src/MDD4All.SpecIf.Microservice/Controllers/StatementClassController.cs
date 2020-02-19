/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    /// <summary>
    /// API controller for SpecIF statement classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/statementClasses")]
    [ApiController]
    public class StatementClassController : Controller
    {
        private ISpecIfMetadataReader _metadataReader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metadataReader"></param>
		public StatementClassController(ISpecIfMetadataReader metadataReader)
        {
            _metadataReader = metadataReader;
        }

        /// <summary>
        /// Returns all statement classes with all available revisions.
        /// </summary>
        /// <returns>All statement classes as a list.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Resource>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<List<StatementClass>> GetAllStatementClasses()
        {
            ActionResult<List<StatementClass>> result = NotFound();

            List<StatementClass> dbResult = _metadataReader.GetAllStatementClasses();

            if(dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }



        /// <summary>
        /// Returns the main/latest revision of the statement class with the given ID. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(StatementClass), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<StatementClass> GetStatementClassById(string id, [FromQuery]string revision)
        {
            ActionResult<StatementClass> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string rev = revision.Replace("%2F", "/");

                    

                    StatementClass statementClass = _metadataReader.GetStatementClassByKey(new Key() { ID = id, Revision = rev });
                    if (statementClass != null)
                    {
                        result = new ObjectResult(statementClass);
                    }
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Returns all statement class revisions for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<StatementClass>), 200)]
        public ActionResult<List<StatementClass>> GetAllStatementClassRevisions(string id)
        {
            ActionResult<List<StatementClass>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<StatementClass> dbResult = _metadataReader.GetAllStatementClassRevisions(id);
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Create a new statement class.
        /// </summary>
        /// <param name="statementClass">The statement class data.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(StatementClass), 200)]
        public ActionResult<StatementClass> CreateStatementClass([FromBody]Resource statementClass)
        {
            ActionResult<StatementClass> result = NotFound();

            return result;
        }

        /// <summary>
        /// Update a statement class.
        /// The subjected ID must exist.
        /// </summary>
        /// <param name="statementClass">The statement class data.</param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(StatementClass), 200)]
        public ActionResult<StatementClass> UpdateStatementClass([FromBody]Resource statementClass)
        {
            ActionResult<StatementClass> result = NotFound();

            return result;
        }

        /// <summary>
        /// Delete a statement class with the given ID.
        /// </summary>
        /// <param name="id">The statement class ID.</param>
        [HttpDelete("{id}")]
        public ActionResult DeleteStatementClass(string id)
        {
            ActionResult result = NotFound();

            return result;
        }


    }
}