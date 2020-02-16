/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [Route("specif/v{version:apiVersion}/projects")]
    [ApiController]
    public class ProjectController : Controller
    {
        /// <summary>
        /// Return all projects; to limit the size only root properties are delivered.
        /// </summary>
        /// <returns>A list of project descriptions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProjectDescriptor>), 200)]
        public ActionResult<List<ProjectDescriptor>> GetAllProjects()
        {
            List<ProjectDescriptor> result = new List<ProjectDescriptor>();

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Return the project with the given ID; to limit the size only ??? are delivered.
        /// </summary>
        /// <param name="id">The project id.</param>
        /// <returns>The project.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SpecIF.DataModels.SpecIF), 201)]
        public ActionResult<SpecIF.DataModels.SpecIF> GetProjectByID(string id)
        {
            SpecIF.DataModels.SpecIF result = new SpecIF.DataModels.SpecIF();

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Create a project with supplied elements; the supplied ID must be unique in the project scope. 
        /// If no ID is supplied, it is generated before insertion.
        /// </summary>
        /// <param name="value">The SpecIF data defining the project.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateNewProject([FromBody]SpecIF.DataModels.SpecIF value)
        {
            return NotFound();
        }

        /// <summary>
        /// Update a project with the gibven ID by including the data into an existing project. The project with the supplied ID must exist.
        /// </summary>
        /// <param name="id">The project ID.</param>
        /// <param name="value">The SpecIF data to include.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        public ActionResult UpdateProject(string id, [FromBody]SpecIF.DataModels.SpecIF value)
        {
            return NotFound();
        }

        /// <summary>
        /// Delete a project with the given ID.
        /// </summary>
        /// <param name="id">Th ID of the project to delete.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult DeleteProject(string id)
        {
            return NotFound();
        }
    }
}
