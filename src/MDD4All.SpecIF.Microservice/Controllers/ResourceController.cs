/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Mvc;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    /// <summary>
    /// Controller to manage resource data.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/resources")]
    [ApiController]
    public class ResourceController : Controller
    {
		private ISpecIfDataReader _specIfDataReader;
		private ISpecIfDataWriter _specIfDataWriter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="specIfDataReader">The SpecIF data reader.</param>
        /// <param name="specIfDataWriter">The SpecIF data writer.</param>
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
        /// <description>The new resource is added as a new element with a specific revision 
        /// and a specific branch, depended on the given information.
        /// The new resource element is returned as response.</description>
        /// <param name="resource">The resource data.</param>
        /// <param name="projectID">The optional project ID. If a project ID is not given, the data is added to a default project.</param>
        /// <returns>The resulting resource data element.</returns>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ProducesResponseType(typeof(Resource), 201)]
        public ActionResult<Resource> AddNewResource([FromBody]Resource resource, [FromQuery]string projectID)
        {
			ActionResult<Resource> result = _specIfDataWriter.SaveResource(resource, projectID);

            if(result == null)
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Updates a resource. The ID included in the resource data must exist.
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        [Authorize(Roles = "Editor")]
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
        /// Deletes the resource.
        /// </summary>
        /// <param name="id">The resource ID.</param>
        /// <param name="revision">The resource revision.</param>
        /// <param name="mode">?mode=forced results in deleting all directly and indirectly dependant model elements.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteResource(string id, [FromQuery]string revision, [FromQuery]string mode)
        {
            ActionResult result = NotFound();

            return result;
        }

	}
}
