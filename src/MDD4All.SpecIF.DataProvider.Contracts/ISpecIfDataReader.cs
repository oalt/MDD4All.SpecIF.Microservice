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

		Revision GetLatestResourceRevision(string resourceID);

		List<Node> GetAllHierarchies();

		Node GetHierarchyByKey(Key key);

		Revision GetLatestHierarchyRevision(string hierarchyID);

		Statement GetStatementByKey(Key key);

		Revision GetLatestStatementRevision(string statementID);

		List<Statement> GetAllStatementsForResource(Key resourceKey);

		List<Node> GetContainingHierarchyRoots(Key resourceKey);

		byte[] GetFile(string filename);
	}
}
