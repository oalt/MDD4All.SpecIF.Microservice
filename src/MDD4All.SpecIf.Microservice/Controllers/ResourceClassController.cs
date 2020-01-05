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
    /// API controller for SpecIF resource classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/resource-classes")]
    [ApiController]
    public class ResourceClassController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metadataReader"></param>
		public ResourceClassController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

        /// <summary>
        /// Returns all resource classes with all available revisions.
        /// </summary>
        /// <returns>All statement classes as a list.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ResourceClass>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<List<ResourceClass>> GetAllResourceClasses()
        {
            ActionResult<List<ResourceClass>> result = NotFound();

            List<ResourceClass> dbResult = _metadataReader.GetAllResourceClasses();

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }



        /// <summary>
        /// Returns the main/latest revision of the resource class with the given ID. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResourceClass), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<ResourceClass> GetResourceClassById(string id)
        {
            ActionResult<ResourceClass> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                ResourceClass dbResult = _metadataReader.GetResourceClassByKey(new Key(id));

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
        /// Returns all resource class revisions for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<ResourceClass>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<List<ResourceClass>> GetAllResourceClassRevisions(string id)
        {
            ActionResult<List<ResourceClass>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<ResourceClass> dbResult = _metadataReader.GetAllResourceClassRevisions(id);
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Returns the resource class with the specific revision.
        /// </summary>
        /// <param name="id">The resource class ID.</param>
        /// <param name="revision">The resource class revision.</param>
        /// <returns></returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(ResourceClass), 200)]
        [ProducesResponseType(404)]
        public ActionResult<ResourceClass> GetResourceClassRevision(string id, string revision)
        {
            ActionResult<ResourceClass> result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                ResourceClass resourceClass = _metadataReader.GetResourceClassByKey(new Key() { ID = id, Revision = revisionObject });
                if (resourceClass != null)
                {
                    result = new ObjectResult(resourceClass);
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