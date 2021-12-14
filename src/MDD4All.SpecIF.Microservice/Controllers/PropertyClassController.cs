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
    /// API controller for SpecIF property classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/propertyClasses")]
    [ApiController]
    public class PropertyClassController : Controller
    {
		private ISpecIfMetadataReader _metadataReader;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metadataReader"></param>
		public PropertyClassController(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

        /// <summary>
        /// Returns all property classes with all available revisions.
        /// </summary>
        /// <returns>All property classes as a list.</returns>
		[HttpGet]
        [ProducesResponseType(typeof(List<PropertyClass>), 200)]
        [ProducesResponseType(400)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<List<PropertyClass>> GetAllPropertyClasses()
		{
            ActionResult<List<PropertyClass>> result = NotFound();

            List<PropertyClass> dbResult = _metadataReader.GetAllPropertyClasses();

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
            
		}


        /// <summary>
        /// Returns the property class with the given ID. 
        /// </summary>
        /// <param name="id">The property class ID.</param>
        /// <param name="revision">The property class revsion.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PropertyClass), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<PropertyClass> GetPropertyClassById(string id, [FromQuery]string revision)
        {
            ActionResult<PropertyClass> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(revision))
                {
                    string rev = revision.Replace("%2F", "/");
                    
                    PropertyClass propertyClass = _metadataReader.GetPropertyClassByKey(new Key() { ID = id, Revision = rev });
                    if (propertyClass != null)
                    {
                        result = new ObjectResult(propertyClass);
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
        /// Returns all property class revisions for the given ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<PropertyClass>), 200)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<List<PropertyClass>> GetAllPropertyClassRevisions(string id)
        {
            ActionResult<List<PropertyClass>> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                List<PropertyClass> dbResult = _metadataReader.GetAllPropertyClassRevisions(id);
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        /// <summary>
        /// Creates a new property class.
        /// </summary>
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [ProducesResponseType(typeof(PropertyClass), 200)]
        public ActionResult<PropertyClass> CreatePropertyClass([FromBody] PropertyClass propertyClass)
        {
            ActionResult result = BadRequest();

            return result;
        }

        /// <summary>
        /// Updates the property class; the supplied ID must exist.
        /// </summary>
        /// <param name="propertyClass">The property class data to update.</param>
        /// <returns>The updated property class element.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut]
        public ActionResult UpdatePropertyClass([FromQuery]PropertyClass propertyClass)
        {
            ActionResult result = NotFound();

            return result;
        }


        /// <summary>
        /// Deletes the property class; the supplied ID must exist. 
        /// Returns an error if there are dependant model elements. 
        /// </summary>
        /// <param name="id">The property class ID.</param>
        /// <param name="revision">The property class revision.</param>
        /// <param name="mode">Delete mode. ?mode=forced results in deleting all directly and indirectly dependant model elements.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeletePropertyClass(string id, [FromQuery]string revision, [FromQuery]string mode)
        {
            ActionResult result = NotFound();

            return result;
        }
    }
}