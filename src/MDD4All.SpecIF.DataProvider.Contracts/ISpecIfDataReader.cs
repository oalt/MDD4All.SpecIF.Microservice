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

		int GetLatestResourceRevision(string resourceID);

		List<Node> GetAllHierarchies();

		Node GetHierarchyByKey(Key key);

		int GetLatestHierarchyRevision(string hierarchyID);

		Statement GetStatementByKey(Key key);

		int GetLatestStatementRevision(string statementID);

		byte[] GetFile(string filename);
	}
}
