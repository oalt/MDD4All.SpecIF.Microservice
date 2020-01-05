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
    /// API controller for SpecIF property classes.
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/property-classes")]
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
        /// Returns all property classes with alll available revisions.
        /// </summary>
        /// <returns>All property classes as a list.</returns>
		[HttpGet]
        [ProducesResponseType(typeof(List<PropertyClass>), 200)]
        [ProducesResponseType(400)]
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
        /// Returns the main/latest revision of the property class with the given ID. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PropertyClass), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ActionResult<PropertyClass> GetPropertyClassById(string id)
        {
            ActionResult<PropertyClass> result = NotFound();

            if (!string.IsNullOrEmpty(id))
            {
                PropertyClass dbResult = _metadataReader.GetPropertyClassByKey(new Key(id));

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
        /// Returns all property class revisions for the given id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<PropertyClass>), 200)]
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
        /// Returns the property class with the specific revision.
        /// </summary>
        /// <param name="id">The property class ID.</param>
        /// <param name="revision">The property class revision.</param>
        /// <returns></returns>
        [HttpGet("{id}/revisions/{revision}")]
        [ProducesResponseType(typeof(StatementClass), 201)]
        [ProducesResponseType(404)]
        public ActionResult<PropertyClass> GetPropertyClassRevision(string id, string revision)
        {
            ActionResult<PropertyClass> result = NotFound();

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(revision))
            {
                string rev = revision.Replace("%2F", "/");

                Revision revisionObject = new Revision(rev);

                PropertyClass propertyClass = _metadataReader.GetPropertyClassByKey(new Key() { ID = id, Revision = revisionObject });
                if (propertyClass != null)
                {
                    result = new ObjectResult(propertyClass);
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