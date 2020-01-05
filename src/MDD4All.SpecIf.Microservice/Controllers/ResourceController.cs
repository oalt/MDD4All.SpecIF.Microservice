/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Mvc;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Annotations;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/resources")]
    [ApiController]
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

        /// <summary>
        /// Returns the latest version of the resource with the given ID.
        /// </summary>
        /// <param name="id">The resource ID.</param>
        /// <returns>The resource data.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Resource), 200)]
        public ActionResult<Resource> GetResourceById(string id)
		{
			return GetResourceRevision(id, Key.LATEST_REVISION.StringValue);
		}

        /// <summary>
        /// Returns a list of all revisions for the resource with the given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<Resource>), 200)]
        public ActionResult<List<Resource>> GetAllResourceRevisions(string id)
        {
            ActionResult<List<Resource>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<Resource> dbResult = _specIfDataReader.GetAllResourceRevisions(id);
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
        /// Returns a specific revision for the resource with the given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(Resource), 201)]
        public ActionResult<Resource> GetResourceRevision(string id, string revision)
        {
            ActionResult result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                Resource resource = _specIfDataReader.GetResourceByKey(new Key() { ID = id, Revision = revisionObject });
                if (resource != null)
                {
                    result = new ObjectResult(resource);
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Returns the latest revision for the resource with the given ID in the given branch.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
		[HttpGet("{id}/branches/{branch}")]
        [ProducesResponseType(typeof(Resource), 200)]
        public ActionResult<Resource> GetLatestResourceRevisionInBranch(string id, string branch)
		{
            ActionResult result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(branch))
            {
                Revision revision = _specIfDataReader.GetLatestResourceRevisionForBranch(id, branch);

                Resource resource = _specIfDataReader.GetResourceByKey(new Key() { ID = id, Revision = revision });
                if (resource != null)
                {
                    result = new ObjectResult(resource);
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }


        /// <summary>
        /// Adds a new resource to the SpecIF repository.
        /// </summary>
        /// <description>The new resource is added as a new element wit a specific revision 
        /// and a specific branch, dependend on the given information.
        /// The new resource element is returned as response.</description>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(Resource), 201)]
        public ActionResult<Resource> AddNewResource([FromBody]Resource resource)
        {
			ActionResult<Resource> result = _specIfDataWriter.SaveResource(resource);

            if(result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

		

	}
}
