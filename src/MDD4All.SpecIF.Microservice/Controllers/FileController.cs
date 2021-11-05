/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/files")]
    [ApiController]
    public class FileController : Controller
    {
        /// <summary>
        /// Returns all file descriptions for all available files in all revisions.
        /// </summary>
        /// <param name="projectID">An optional project ID as filter.</param>
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
        /// <param name="id">The file ID.</param>
        /// <returns>All available file descriptions for all revisions with this file ID.</returns>
        [HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<File>), 200)]
        public ActionResult<List<File>> GetAllFileRevisions(string id)
        {
            ActionResult<List<File>> result = NotFound();


            return result;
        }

        /// <summary>
        /// Creates a file; the supplied ID must be unique.
        /// </summary>
        /// <param name="file"></param>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        public void CreateNewFile(IFormFile file)
        {
        }

        /// <summary>
        /// Updates the file; the supplied ID must exist.
        /// </summary>
        /// <param name="file"></param>
        [Authorize(Roles = "Editor")]
        [HttpPut]
        public void UpdateFile(IFormFile file)
        {
        }

        /// <summary>
        /// Deletes the file; the supplied ID must exist. 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public void DeleteFile(string id, [FromQuery]string revision)
        {
        }
    }
}
