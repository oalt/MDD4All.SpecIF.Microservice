using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/statements")]
    [ApiController]
    public class StatementController : Controller
    {
        private ISpecIfDataReader _specIfDataReader;
        private ISpecIfDataWriter _specIfDataWriter;
    
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="specIfDataReader"></param>
        /// <param name="specIfDataWriter"></param>
        public StatementController(ISpecIfDataReader specIfDataReader, ISpecIfDataWriter specIfDataWriter)
        {
            _specIfDataReader = specIfDataReader;
            _specIfDataWriter = specIfDataWriter;
        }

        /// <summary>
        /// Returns all statements with all available revisions.
        /// With the optional project ID only the satatements for the project are returned.
        /// </summary>
        /// <param name="projectID">The optional project ID to filter statements by project.</param>
        /// <param name="objectID">The optional objectID to filter statements only targeting the element with the given id.</param>
        /// <param name="objectRevision">An optional object revision. Only usefull toghther with objectId.</param>
        /// <param name="subjectID">The optional subjectID to filter statements only sourcing the element with the given id.</param>
        /// <param name="subjectRevision">An optional subject revision. Only usefull toghther with subjectID.</param>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        public ActionResult<List<Statement>> GetAllStatements([FromQuery]string projectID, 
                                                              [FromQuery]string subjectID,
                                                              [FromQuery]string subjectRevision,
                                                              [FromQuery]string objectID,
                                                              [FromQuery]string objectRevision)
        {
            ActionResult<List<Statement>> result = NotFound();

            return result;
        }


        /// <summary>
        /// Returns the statement with the given ID.
        /// </summary>
        /// <param name="id">The statement ID.</param>
        /// <param name="revision">The statement revision.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Statement), 200)]
        public ActionResult<Statement> GetStatementById(string id, [FromQuery]string revision)
        {
            ActionResult result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string rev = revision.Replace("%2F", "/");

                    Statement statement = _specIfDataReader.GetStatementByKey(new Key() { ID = id, Revision = rev });
                    if (statement != null)
                    {
                        result = new ObjectResult(statement);
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
        /// Returns all available revisions for the statement wit the given ID.
        /// </summary>
        /// <param name="id">The statement id.</param>
        /// <returns>A list of statement revsions.</returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<Resource>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetAllStatementRevisions(string id)
        {
            ActionResult<List<Statement>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<Statement> dbResult = _specIfDataReader.GetAllStatementRevisions(id);
                if (dbResult != null && dbResult.Count > 0)
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
        /// Create a statement; the supplied ID must be unique in the project scope.
        /// </summary>
        /// <param name="statement">The statement to create.</param>
        /// <returns>The created statement data.</returns>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ProducesResponseType(typeof(Statement), 201)]
        [ProducesResponseType(400)]
        public ActionResult<Statement> CreateNewStatement([FromBody]Statement statement)
        {

            ActionResult<Statement> result = _specIfDataWriter.SaveStatement(statement);

            if(result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Update a new statement. If a statement with the given ID is still existant, a new revision is created automatically.
        /// </summary>
        /// <param name="statemenet">The statement to update.</param>
        /// <returns>The created statement data (perhaps with modified revision data).</returns>
        [Authorize(Roles = "Editor")]
        [HttpPut]
        [ProducesResponseType(typeof(Statement), 201)]
        [ProducesResponseType(400)]
        public ActionResult<Statement> UpdateStatement([FromBody]Statement statemenet)
        {

            ActionResult<Statement> result = _specIfDataWriter.SaveStatement(statemenet);

            if (result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Delete the statement; the supplied ID must exist. Return an error if there are depending model elements.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <param name="mode">?mode=forced results in deleting all directly and indirectly depending model elements.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteStatement(string id, [FromQuery]string revision, [FromQuery]string mode)
        {
            ActionResult result = NotFound();

            return result;
        }

    }
}
