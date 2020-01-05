/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    [Produces("application/json")]
    [ApiVersion("1.0")]
    [Route("specif/v{version:apiVersion}/nodes")]
    [ApiController]
    public class NodeController : Controller
    {
        private ISpecIfDataWriter _specIfDataWriter;
        private ISpecIfDataReader _specIfDataReader;


        public NodeController(ISpecIfDataWriter dataWriter, ISpecIfDataReader dataReader)
        {
            _specIfDataWriter = dataWriter;
            _specIfDataReader = dataReader;
        }

        /// <summary>
        /// Returns all available root nodes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("root-nodes")]
        [ProducesResponseType(typeof(List<Node>), 200)]
        public ActionResult<List<Node>> GetRootNodes()
        {
            ActionResult<List<Node>> result = NotFound();

            result = _specIfDataReader.GetAllHierarchyRootNodes();

            return result;
        }

        /// <summary>
        /// Returns one level of child nodes for a parent node.
        /// </summary>
        /// <returns>The child node for the parent with the given ID.</returns>
        [HttpGet("{parentNodeId}/child-nodes")]
        [ProducesResponseType(typeof(List<Node>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ActionResult<List<Node>> GetChildNodes(string parentNodeId)
        {
            ActionResult<List<Node>> result = NotFound();

            if (!string.IsNullOrEmpty(parentNodeId))
            {

                List<Node> dbResult = _specIfDataReader.GetChildNodes(new Key { ID = parentNodeId, Revision = Key.FIRST_MAIN_REVISION });

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
        /// Add a new node to a parent node with given ID.
        /// </summary>
        /// <param name="parentNodeId">The parent node ID.</param>
        /// <param name="node">The new child node.</param>
		[HttpPost("parent/{parentNodeId}")]
        public void AddChildNode(string parentNodeId, [FromBody]Node node)
        {
            _specIfDataWriter.AddNode(parentNodeId, node);
        }

        /// <summary>
        /// Updates an existing node.
        /// </summary>
        /// <param name="node">The node to update.</param>
        [HttpPut]
        [ProducesResponseType(201)]
        public ActionResult UpdateNode([FromBody] Node node)
        {
            ActionResult result = new OkResult();

            if(!string.IsNullOrEmpty(node.ID) && node.Revision != null)
            {
                

                _specIfDataWriter.SaveNode(node);
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }


        /// <summary>
        /// Delete a node with the given ID.
        /// </summary>
        /// <param name="id">The node ID.</param>
        [HttpDelete("{id}")]
        public void DeleteNode(string id)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Moves an existion node and all child nodes to a new parent.
        /// </summary>
        /// <param name="nodeId">The id of the node to move.</param>
        /// <param name="newParentId">The id of the new parent.</param>
        /// <param name="position">The desired node position.</param>
        [HttpPut("move/{nodeId}/to/{newParentId}/position/{position}")]
        [SwaggerOperation(OperationId = "{FC0EF587-8E82-41B6-9149-4EF87EE21E6F}")]
        public void MoveNode(string nodeId, string newParentId, string position)
        {
            throw new NotImplementedException();
        }
    }
}