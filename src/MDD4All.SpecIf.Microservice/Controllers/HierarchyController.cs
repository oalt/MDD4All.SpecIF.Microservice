/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Net;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    /// <summary>
    /// Web API controller for Hierarchy elements. 
    /// </summary>
    [Produces("application/json")]
    [ApiVersion("1.0")]
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
        /// Get all hierarchies.
        /// </summary>
        /// <returns></returns>
		[HttpGet]
        [ProducesResponseType(typeof(List<Node>), 200)]
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

            result = _dataReader.GetAllHierarchyRootNodes();

            return result;
        }

        /// <summary>
        /// Get hierarchy with a specific ID.
        /// </summary>
        /// <param name="id">The hierarchy ID.</param>
        /// <param name="revision">The hierarchy revision.</param>
        /// <param name="depth">The maximum depth of child nodes to return. If not set the complete hierarchy depth is returned.</param>
        /// <returns></returns>
		[HttpGet("{id}")]
        [ProducesResponseType(typeof(Node), 200)]
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
        /// Get all revisions from a hierarchy with a specific ID.
        /// </summary>
        /// <param name="id">The hierarchy ID.</param>
        /// <param name="depth">The maximum depth of child nodes to return. If not set the complete hierarchy depth is returned.</param>
        /// <returns></returns>
		[HttpGet("{id}/revisions")]
        [ProducesResponseType(typeof(List<Node>), 200)]
        public ActionResult<List<Node>> GetAllHierarchyRevisions(string id, [FromBody]int depth)
        {
            ActionResult<List<Node>> result = NotFound();

            return result;
        }

        /// <summary>
        /// Add a new hierarchy.
        /// </summary>
        /// <param name="node">The hierarchy data to add.</param>
        /// <param name="parentNodeId">An optional parent node id.</param>
        [Authorize(Roles = "Editor")]
        [HttpPost]
        public void CreateNewHierarchy([FromBody]Node node, [FromQuery]string parentNodeId)
        {
            if (string.IsNullOrEmpty(parentNodeId))
            {
                _dataWriter.AddHierarchy(node);
            }
            else
            {
                _dataWriter.AddNode(parentNodeId, node);
            }
        }



        /// <summary>
        /// Update an existing hierarchy node.
        /// </summary>
        /// <param name="node"></param>
        [Authorize(Roles = "Editor")]
        [HttpPut]
        public void UpdateHierarchy([FromBody]Node node)
        {
            _dataWriter.SaveHierarchy(node);
        }

        /// <summary>
        /// Delete a hierarchy.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revision"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("{id}")]
        public ActionResult DeleteHierarchy(string id, [FromQuery]string revision)
        {
            ActionResult result = NotFound();

            return result;
        }

        /// <summary>
        /// Moves an existing node and all child nodes to a new parent.
        /// </summary>
        /// <param name="nodeId">The id of the node to move.</param>
        /// <param name="newParentId">The id of the new parent.</param>
        /// <param name="newSiblingId">The id of the new sibling. 
        /// If nothing is set the node will be the first element in the new location list.
        /// </param>
        [Authorize(Roles = "Editor")]
        [HttpPut("move")]
        public ActionResult MoveNode([FromQuery]string nodeId, [FromQuery]string newParentId, [FromQuery]string newSiblingId)
        {
            ActionResult result = new OkResult();

            if (!string.IsNullOrEmpty(nodeId) && !string.IsNullOrEmpty(newParentId))
            {
                try
                {
                    _dataWriter.MoveNode(nodeId, newParentId, newSiblingId);
                }
                catch (Exception exception)
                {
                    result = new BadRequestObjectResult(exception);
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
