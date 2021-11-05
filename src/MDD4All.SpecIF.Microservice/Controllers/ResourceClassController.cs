/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIF.Microservice.Controllers
{

    /// <summary>
    /// API controller for SpecIF resource classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/resourceClasses")]
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
        /// Returns the resource class with the given ID. 
        /// </summary>
        /// <param name="id">The resource class ID.</param>
        /// <param name="revision">The resource class revision.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResourceClass), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<ResourceClass> GetResourceClassById(string id, [FromQuery]string revision)
        {
            ActionResult<ResourceClass> result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");
                
                ResourceClass resourceClass = _metadataReader.GetResourceClassByKey(new Key() { ID = id, Revision = rev });
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

        /// <summary>
        /// Returns all resource class revisions for the given ID.
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
        /// Creates a new resource class.
        /// </summary>
        /// <param name="resourceClass">The resource class data.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(typeof(ResourceClass), 200)]
        public ActionResult<ResourceClass> CreateResourceClass([FromBody]ResourceClass resourceClass)
        {
            ActionResult<ResourceClass> result = NotFound();

            return result;
        }

        /// <summary>
        /// Updates a resource class.
        /// The subjected ID must exist.
        /// </summary>
        /// <param name="resourceClass">The statement class data.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        [ProducesResponseType(typeof(ResourceClass), 200)]
        public ActionResult<ResourceClass> UpdateResourceClass([FromBody]Resource resourceClass)
        {
            ActionResult<ResourceClass> result = NotFound();

            return result;
        }

        /// <summary>
        /// Deletes a resource class with the given ID.
        /// </summary>
        /// <param name="id">The resource class ID.</param>
        /// <param name="revision">The revision ID.</param>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteResourceClass(string id, [FromQuery]string revision)
        {
            ActionResult result = NotFound();

            return result;
        }
    }
}