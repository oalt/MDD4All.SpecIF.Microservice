/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfDataReader : ISpecIfDataProviderBase
	{ 

		Resource GetResourceByKey(Key key);

        List<Resource> GetAllResourceRevisions(string resourceID);

		string GetLatestResourceRevisionForBranch(string resourceID, string branchName);

		List<Node> GetAllHierarchies();

        List<Node> GetAllHierarchyRootNodes();

		Node GetHierarchyByKey(Key key);

        List<Node> GetChildNodes(Key parentNodeKey);

        Node GetNodeByKey(Key key);

        Node GetParentNode(Key childNode);

		string GetLatestHierarchyRevision(string hierarchyID);

		Statement GetStatementByKey(Key key);

        List<Statement> GetAllStatementRevisions(string statementID);

        string GetLatestStatementRevision(string statementID);

        List<Statement> GetAllStatements();

		List<Statement> GetAllStatementsForResource(Key resourceKey);

		List<Node> GetContainingHierarchyRoots(Key resourceKey);

		byte[] GetFile(string filename);
	}
}
