﻿/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Base.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    [ApiVersion("1.1")]
    [Produces("application/json")]
    [Route("specif/v{version:apiVersion}/projects")]
    [ApiController]
    public class ProjectController : Controller
    {
        private ISpecIfDataReader _specIfDataReader;
        private ISpecIfDataWriter _specIfDataWriter;
        private ISpecIfMetadataReader _metadataReader;
        private ISpecIfMetadataWriter _metadataWriter;

        public ProjectController(ISpecIfDataReader specIfDataReader,
                                 ISpecIfDataWriter specIfDataWriter,
                                 ISpecIfMetadataReader metadataReader,
                                 ISpecIfMetadataWriter metadataWriter)
        {
            _specIfDataReader = specIfDataReader;
            _specIfDataWriter = specIfDataWriter;
            _metadataReader = metadataReader;
            _metadataWriter = metadataWriter;
        }

        /// <summary>
        /// Returns all projects; to limit the size only root properties are delivered.
        /// </summary>
        /// <returns>A list of project descriptions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProjectDescriptor>), 200)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<List<ProjectDescriptor>> GetAllProjects()
        {
            List<ProjectDescriptor> result = new List<ProjectDescriptor>();

            result = _specIfDataReader.GetProjectDescriptions();

            return new OkObjectResult(result);
        }

        /// <summary>
        /// Returns the project with the given ID.
        /// </summary>
        /// <param name="id">The project ID.</param>
        /// <param name="hierarchyFilter">An optional comma seperated list of hierarchy root node IDs to limit the output of selected hierarchies.</param>
        /// <param name="includeMetedata">Set to true if the metadata should be included (e.g. resource classes etc.)</param>
        /// <returns>The project as SpecIF JSON.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SpecIF.DataModels.SpecIF), 201)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<SpecIF.DataModels.SpecIF> GetProjectByID(string id, 
                                                                    [FromQuery] string hierarchyFilter,
                                                                    [FromQuery] bool includeMetedata = true)
        {
            ActionResult result = BadRequest();

            try
            {
                if (id != null)
                {
                    List<Key> hierarchyKeys = new List<Key>();

                    if (hierarchyFilter != null)
                    {
                        char separator = ',';

                        string[] splitResult = hierarchyFilter.Split(separator);

                        foreach(string hierarchyId in splitResult)
                        {
                            if(!string.IsNullOrEmpty(hierarchyId))
                            {
                                hierarchyKeys.Add(new Key(hierarchyId));
                            }
                        }
                    }


                    SpecIF.DataModels.SpecIF specif = _specIfDataReader.GetProject(_metadataReader, id, hierarchyKeys, includeMetedata);
                    result = new OkObjectResult(specif);
                }
            }
            catch(Exception exception)
            {
                if(exception is ProjectNotFoundException)
                {
                    result = NotFound();
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a project with supplied elements; the supplied ID must be unique in the project scope. 
        /// If no ID is supplied, it is created before insertion.
        /// </summary>
        /// <param name="specIF">The SpecIF data defining the project.</param>
        /// <returns></returns>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ProducesResponseType(200)]
        public ActionResult CreateNewProject([FromBody]SpecIF.DataModels.SpecIF specIF,
                                             [FromQuery]string integrationId)
        {
            ActionResult result = BadRequest();

            if(specIF != null)
            {
                _specIfDataWriter.AddProject(_metadataWriter, specIF, integrationId);
                result = new OkResult();
            }

            return result;
        }

        /// <summary>
        /// Updates a project with the given ID by including the data into an existing project. The project with the supplied ID must exist.
        /// </summary>
        /// <param name="value">The SpecIF data to include.</param>
        /// <returns></returns>
        [Authorize(Roles = "Editor")]
        [HttpPut]
        [ProducesResponseType(200)]
        public ActionResult UpdateProject([FromBody]SpecIF.DataModels.SpecIF value)
        {
            ActionResult result = BadRequest();

            if(value != null)
            {
                try
                {
                    _specIfDataWriter.UpdateProject(_metadataWriter, value);
                    result = new OkResult();
                }
                catch(Exception exception)
                {
                    if(exception is ProjectNotFoundException)
                    {
                        result = NotFound();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes a project with the given ID.
        /// </summary>
        /// <param name="id">The ID of the project to delete.</param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteProject(string id)
        {
            return NotFound();
        }
    }
}
