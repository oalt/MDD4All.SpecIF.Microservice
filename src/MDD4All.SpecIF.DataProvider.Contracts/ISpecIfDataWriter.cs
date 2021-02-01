/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfDataWriter : ISpecIfDataProviderBase
	{
        void AddProject(ISpecIfMetadataWriter metadataWriter, 
                        SpecIF.DataModels.SpecIF project, 
                        string integrationID = null);

        void UpdateProject(ISpecIfMetadataWriter metadataWriter, 
                           SpecIF.DataModels.SpecIF project);

        void DeleteProject(string projectID);

		void AddResource(Resource resource);

		Resource SaveResource(Resource resource, string projectID = null);

        Resource UpdateResource(Resource resource);

		void AddHierarchy(Node hierarchy, string projectID = null);

		Node UpdateHierarchy(Node hierarchyToUpdate, string parentID = null, string predecessorID = null);

		void AddStatement(Statement statement);

		Statement SaveStatement(Statement statement, string projectID = null);

		void AddNodeAsFirstChild(string parentNodeID, Node newNode);

        /// <summary>
        /// Add node as predecessor of existing node in the same hierarchy level.
        /// </summary>
        /// <param name="predecessorID">The node id of an existing predecessor.</param>
        /// <param name="newNode">The new node to add.</param>
        void AddNodeAsPredecessor(string predecessorID, Node newNode);

        void MoveNode(string nodeID, string newParentID, string newSiblingId);

        void DeleteNode(string nodeID);

		void InitializeIdentificators();

		void SaveIdentificators();

	}
}
