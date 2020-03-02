/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfDataWriter : ISpecIfDataProviderBase
	{
		Resource CreateResource(Key resourceClassKey);

		void AddResource(Resource resource);

		Resource SaveResource(Resource resource);

        Resource UpdateResource(Resource resource);

		void AddHierarchy(Node hierarchy);

		Node SaveHierarchy(Node hierarchyToUpdate);

		void AddStatement(Statement statement);

		Statement SaveStatement(Statement statement);

		void AddNode(string parentNodeID, Node newNode);

		Node SaveNode(Node nodeToUpdate);

        void MoveNode(string nodeID, string newParentID, string newSiblingId);

		long GetNextSpecIfIdentifier(string prefix);

		void InitializeIdentificators();

		void SaveIdentificators();

	}
}
