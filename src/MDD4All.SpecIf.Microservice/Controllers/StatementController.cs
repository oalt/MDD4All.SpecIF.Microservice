using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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
        /// Returns the main/latest revision of the statement nwith the given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Statement), 200)]
        public ActionResult<Statement> GetStatementById(string id)
        {
            return GetStatementRevision(id, Key.LATEST_REVISION.StringValue);
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
        /// Returns a specific revision for the statement with the given id.
        /// </summary>
        /// <param name="id">The statement id.</param>
        /// <param name="revision">The statement revision.</param>
        /// <returns>The statement with the given revision.</returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(Statement), 201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<Statement> GetStatementRevision(string id, string revision)
        {
            ActionResult result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                Statement statement = _specIfDataReader.GetStatementByKey(new Key() { ID = id, Revision = revisionObject });
                if (statement != null)
                {
                    result = new ObjectResult(statement);
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Returns all statements for the subject resource with the given ID and the main/latest subject revision.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <returns></returns>
        [HttpGet("subject/{subjectId}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetStatementsForMainLatestSubject(string subjectId)
        {
            ActionResult<List<Statement>> result = NotFound();



            return result;
        }

        /// <summary>
        /// Returns all statements for the object resource with the given ID and the main/latest object revision.
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        [HttpGet("object/{objectId}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetStatementsForMainLatestObject(string objectId)
        {
            ActionResult<List<Statement>> result = NotFound();



            return result;
        }

        /// <summary>
        /// Returns all statements for the subject resource with the given ID and the given subject revision.
        /// </summary>
        /// <param name="subjectId"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [HttpGet("subject/{subjectId}/subject-revision/{revision}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetStatementsBySubject(string subjectId, string revision)
        {
            ActionResult<List<Statement>> result = NotFound();

            

            return result;
        }

        /// <summary>
        /// Returns all statements for the object resource with the given ID and the given object revision.
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [HttpGet("object/{objectId}/object-revision/{revision}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetStatementsByObject(string objectId, string revision)
        {
            ActionResult<List<Statement>> result = NotFound();



            return result;
        }

        /// <summary>
        /// Returns all statements for a resource with a given ID and main/latest revision - resource is uses as subject OR object for the statement.
        /// </summary>
        /// <param name="resourceId">The resource element ID.</param>
        /// <returns>A list of statements for the resource.</returns>
        [HttpGet("resource/{resourceId}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetAllStatementsForMainLatestResource(string resourceId)
        {
            ActionResult<List<Statement>> result = NotFound();

            return result;
        }

        /// <summary>
        /// Returns all statements for a resource with a given ID and teh given revision - resource is uses as subject OR object for the statement.
        /// </summary>
        /// <param name="resourceId">The resource element ID.</param>
        /// <param name="revision">The resource element revision.</param>
        /// <returns>A list of statements for the resource.</returns>
        [HttpGet("resource/{resourceId}/resource-revision/{revision}")]
        [ProducesResponseType(typeof(List<Statement>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Statement>> GetAllStatementsForResource(string resourceId, string revision)
        {
            ActionResult<List<Statement>> result = NotFound();

            if (!string.IsNullOrEmpty(resourceId) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                List<Statement> dbResult = _specIfDataReader.GetAllStatementsForResource(new Key() { ID = resourceId, Revision = revisionObject });

                if (dbResult != null)
                {
                    result = dbResult;
                }
            }

            return result;
        }

        /// <summary>
        /// Add a new statement. If a statement with the given ID is still existant, a new revision is created automatically.
        /// </summary>
        /// <param name="statemenet">The statement to create.</param>
        /// <returns>The created statement data (perhaps with modified id and revision data).</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Statement), 201)]
        [ProducesResponseType(400)]
        public ActionResult<Statement> AddNewStatement([FromBody]Statement statemenet)
        {

            ActionResult<Statement> result = _specIfDataWriter.SaveStatement(statemenet);

            if(result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

    }
}
