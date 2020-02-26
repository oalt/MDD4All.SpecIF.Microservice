/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/files")]
    [ApiController]
    public class FileController : Controller
    {
        /// <summary>
        /// Return all file descriptions for all available files in all revisions.
        /// </summary>
        /// <param name="projectID">An optional projectID as filter.</param>
        /// <returns>All file descriptions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<File>), 200)]
        public ActionResult<List<File>> GetAllFiles([FromQuery]string projectID)
        {
            ActionResult<List<File>> result = NotFound();


            return result;
        }

        /// <summary>
        /// Returns a specific file.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public FileContentResult GetFileById(string id, [FromQuery]string revision)
        {
            FileContentResult result = null;

            return result;
        }

        /// <summary>
        /// Returns all available file revisions as SpecIF file descriptions.
        /// </summary>
        /// <param name="id">The file id.</param>
        /// <returns>All available file descriptions for all revisions with this file ID.</returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<File>), 200)]
        public ActionResult<List<File>> GetAllFileRevisions(string id)
        {
            ActionResult<List<File>> result = NotFound();


            return result;
        }

        /// <summary>
        /// Create a file; the supplied ID must be unique.
        /// </summary>
        /// <param name="file"></param>
        [HttpPost]
        public void CreateNewFile(IFormFile file)
        {
        }

        /// <summary>
        /// Update the file; the supplied ID must exist.
        /// </summary>
        /// <param name="file"></param>
        [HttpPut]
        public void UpdateFile(IFormFile file)
        {
        }

        /// <summary>
        /// Delete the file; the supplied ID must exist. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        [HttpDelete("{id}")]
        public void DeleteFile(string id, [FromQuery]string revision)
        {
        }
    }
}
