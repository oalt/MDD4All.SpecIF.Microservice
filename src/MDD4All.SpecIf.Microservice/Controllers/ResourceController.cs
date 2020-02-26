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
        /// Returns all resources with all available revisions.
        /// </summary>
        /// <param name="projectID">An optional project ID. The endpoint then returns only resources for the given project.</param>
        /// <returns>The resource data.</returns>
        [HttpGet()]
        [ProducesResponseType(typeof(List<Resource>), 200)]
        public ActionResult<List<Resource>> GetAllResources([FromQuery]string projectID)
        {
            return new List<Resource>();
        }

        /// <summary>
        /// Returns the latest version of the resource with the given ID.
        /// </summary>
        /// <param name="id">The resource ID.</param>
        /// <param name="revision">The resource revision.</param>
        /// <returns>The resource data.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Resource), 200)]
        public ActionResult<Resource> GetResourceById(string id, [FromQuery]string revision)
		{
            ActionResult result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string rev = revision.Replace("%2F", "/");
                    
                    Resource resource = _specIfDataReader.GetResourceByKey(new Key() { ID = id, Revision = rev });
                    if (resource != null)
                    {
                        result = new ObjectResult(resource);
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
        /// Returns a list of all revisions for the resource with the given ID.
        /// </summary>
        /// <param name="id">The resource ID.</param>
        /// <returns>All available revisions for this resource.</returns>
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
        /// Adds a new resource to the SpecIF repository.
        /// </summary>
        /// <description>The new resource is added as a new element wit a specific revision 
        /// and a specific branch, depended on the given information.
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

        /// <summary>
        /// Update a resource. The ID included in the resource data must exist.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [HttpPut]
        [ProducesResponseType(typeof(Resource), 201)]
        public ActionResult<Resource> UpdateResource([FromBody]Resource resource)
        {
            ActionResult<Resource> result = _specIfDataWriter.UpdateResource(resource);

            if (result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Delete the resource.
        /// </summary>
        /// <param name="id">The resource ID.</param>
        /// <param name="revision">The resource revision.</param>
        /// <param name="mode">?mode=forced results in deleting all directly and indirectly depending model elements.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult DeleteResource(string id, [FromQuery]string revision, [FromQuery]string mode)
        {
            ActionResult result = NotFound();

            return result;
        }

	}
}
