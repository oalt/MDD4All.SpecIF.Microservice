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

		Node SaveHierarchy(Node hierarchyToUpdate);

		void AddStatement(Statement statement);

		Statement SaveStatement(Statement statement, string projectID = null);

		void AddNode(string parentNodeID, Node newNode);

        void MoveNode(string nodeID, string newParentID, string newSiblingId);

		long GetNextSpecIfIdentifier(string prefix);

		void InitializeIdentificators();

		void SaveIdentificators();

	}
}
