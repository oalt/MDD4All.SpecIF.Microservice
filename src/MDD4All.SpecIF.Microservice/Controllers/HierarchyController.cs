﻿/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Net;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIF.Microservice.Controllers
{
    /// <summary>
    /// Web API controller for hierarchy elements. 
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.1")]
    [Route("specif/v{version:apiVersion}/hierarchies")]
    [ApiController]
    public class HierarchyController : Controller
    {
		ISpecIfDataReader _dataReader;

		ISpecIfDataWriter _dataWriter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="dataWriter">The data writer.</param>
		public HierarchyController(ISpecIfDataReader dataReader, ISpecIfDataWriter dataWriter)
		{
			_dataReader = dataReader;
			_dataWriter = dataWriter;
		}

		/// <summary>
        /// Gets all hierarchies.
        /// </summary>
        /// <returns></returns>
		[HttpGet]
        [ProducesResponseType(typeof(List<Node>), 200)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<List<Node>> GetAllHierarchies([FromQuery]string project, [FromQuery]bool rootNodesOnly)
        {
            ActionResult<List<Node>> result = NotFound();

            if(rootNodesOnly == true)
            {
                result = GetRootNodes(project);
            }
            else
            {
                result = _dataReader.GetAllHierarchies();
            }

            return result;
        }

        private ActionResult<List<Node>> GetRootNodes(string project)
        {
            ActionResult<List<Node>> result = NotFound();

            result = _dataReader.GetAllHierarchyRootNodes(project);

            return result;
        }

        /// <summary>
        /// Gets a hierarchy with a specific ID.
        /// </summary>
        /// <param name="id">The hierarchy ID.</param>
        /// <param name="revision">The hierarchy revision.</param>
        /// <param name="depth">The maximum depth of child nodes to return. If not set the complete hierarchy depth is returned.</param>
        /// <returns></returns>
		[HttpGet("{id}")]
        [ProducesResponseType(typeof(Node), 200)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<Node> GetHierarchyById(string id, [FromQuery]string revision, [FromQuery]int depth)
		{
			ActionResult<Node> result = BadRequest();

            string rev = null;

            if (revision != null)
            {
                rev = WebUtility.UrlDecode(revision);
            }

            if (!string.IsNullOrEmpty(id) && _dataReader != null)
			{
                Key key = new Key(id, rev);

				Node hierarchy = _dataReader.GetHierarchyByKey(key);

				if (hierarchy != null)
				{
					result = new OkObjectResult(hierarchy);
				}
			}

			return result;
		}

        /// <summary>
        /// Gets all revisions from a hierarchy with a specific ID.
        /// </summary>
        /// <param name="id">The hierarchy ID.</param>
        /// <param name="depth">The maximum depth of child nodes to return. If not set the complete hierarchy depth is returned.</param>
        /// <returns></returns>
		[HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<Node>), 200)]
        [Authorize(Policy = "unregisteredReader")]
        public ActionResult<List<Node>> GetAllHierarchyRevisions(string id, [FromQuery] int depth)
        {
            ActionResult<List<Node>> result = NotFound();

            return result;
        }

        /// <summary>
        /// Creates a hierarchy (sub-tree) with supplied nodes; the supplied ID must be unique. 
        /// If no ID is supplied, it is created before insertion. 
        /// Query ?parent=nodeId - the sub-tree will be inserted as first child; 
        /// query ?predecessor=nodeId - the sub-tree will be inserted after the specified node; 
        /// no query - the sub-tree will be inserted as first element at root level. 
        /// Without query string, the node (sub-tree) is inserted as first element at root level.
        /// </summary>
        /// <param name="node">The hierarchy data to add.</param>
        /// <param name="parent">An optional parent node ID. The sub-tree will be inserted as first child.</param>
        /// <param name="predecessor">An optional predecessor node ID. The sub-tree will be inserted after the specified node.</param>
        /// <param name="projectId">The project ID. If the ID is given, the new hierarchy will be added to the specific project. 
        /// Only useful for new hierarchies - no parent or predecessor given.
        /// </param>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        public void CreateNewHierarchy([FromBody]Node node, 
                                       [FromQuery]string parent,
                                       [FromQuery]string predecessor,
                                       [FromQuery]string projectId)
        {
            if (string.IsNullOrEmpty(parent) && string.IsNullOrEmpty(predecessor))
            {
                _dataWriter.AddHierarchy(node, projectId);
            }
            else
            {
                if (string.IsNullOrEmpty(predecessor) && !string.IsNullOrEmpty(parent))
                {
                    _dataWriter.AddNodeAsFirstChild(parent, node);
                }
                else if(!string.IsNullOrEmpty(predecessor))
                {
                    _dataWriter.AddNodeAsPredecessor(predecessor, node);
                }
            }
        }



        /// <summary>
        /// Updates an existing hierarchy node.
        /// The supplied ID must exist somewhere in any hierarchy. 
        /// Query ?parent=nodeId - the sub-tree will be moved and inserted as first child; 
        /// query ?predecessor=nodeId - the sub-tree will be moved and inserted after the specified node. 
        /// Without query string, the node (sub-tree) is not moved.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="parent">An optional parent node ID. The sub-tree will be inserted as first child.</param>
        /// <param name="predecessor">An optional predecessor node ID. The sub-tree will be inserted after the specified node.</param>
        [Authorize(Roles = "Editor")]
        [HttpPut]
        public void UpdateHierarchy([FromBody]Node node,
                                    [FromQuery]string parent,
                                    [FromQuery]string predecessor)
        {
            _dataWriter.UpdateHierarchy(node, parent, predecessor);
        }

        /// <summary>
        /// Deletes a hierarchy.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [Authorize(Roles = "Editor")]
        [HttpDelete("{id}")]
        public ActionResult DeleteHierarchy(string id, [FromQuery]string revision)
        {
            ActionResult result = Ok();

            _dataWriter.DeleteNode(id);

            return result;
        }

        ///// <summary>
        ///// Moves an existing node and all child nodes to a new parent.
        ///// </summary>
        ///// <param name="nodeId">The ID of the node to move.</param>
        ///// <param name="newParentId">The ID of the new parent.</param>
        ///// <param name="newSiblingId">The ID of the new sibling. 
        ///// If nothing is set the node will be the first element in the new location list.
        ///// </param>
        //[Authorize(Roles = "Editor")]
        //[HttpPut("move")]
        //public ActionResult MoveNode([FromQuery]string nodeId, [FromQuery]string newParentId, [FromQuery]string newSiblingId)
        //{
        //    ActionResult result = new OkResult();

        //    if (!string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(newParentId))
        //    {
        //        try
        //        {
        //            _dataWriter.MoveNode(nodeId, newParentId, newSiblingId);
        //        }
        //        catch (Exception exception)
        //        {
        //            result = new BadRequestObjectResult(exception);
        //        }

        //    }
        //    else
        //    {
        //        result = new BadRequestResult();
        //    }


        //    return result;
        //}
    }
}
