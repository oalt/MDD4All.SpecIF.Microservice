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

		string GetLatestResourceRevision(string resourceID);

		List<Node> GetAllHierarchies();

		Node GetHierarchyByKey(Key key);

		string GetLatestHierarchyRevision(string hierarchyID);

		Statement GetStatementByKey(Key key);

		string GetLatestStatementRevision(string statementID);

		byte[] GetFile(string filename);
	}
}
