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
    /// <summary>
    /// API controller for SpecIF statement classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/statement-classes")]
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
        [ProducesResponseType(typeof(List<StatementClass>), 200)]
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
        public ActionResult<StatementClass> GetStatementClassById(string id)
        {
            ActionResult<StatementClass> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                StatementClass dbResult = _metadataReader.GetStatementClassByKey(new Key(id));

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
        /// Returns the statement class with the specific revision.
        /// </summary>
        /// <param name="id">The statement class ID.</param>
        /// <param name="revision">The statement class revision.</param>
        /// <returns></returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(StatementClass), 200)]
        [ProducesResponseType(404)]
        public ActionResult<StatementClass> GetStatementClassRevision(string id, string revision)
        {
            ActionResult<StatementClass> result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                StatementClass statementClass = _metadataReader.GetStatementClassByKey(new Key() { ID = id, Revision = revisionObject });
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