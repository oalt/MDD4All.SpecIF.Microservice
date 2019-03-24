/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Service;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public abstract class AbstractSpecIfDataReader : ISpecIfDataReader
	{
		public ISpecIfServiceDescription DataSourceDescription { get; set; }

		public abstract List<Node> GetAllHierarchies();
		public abstract byte[] GetFile(string filename);
		public abstract Node GetHierarchyByKey(Key key);
		public abstract int GetLatestHierarchyRevision(string hierarchyID);
		public abstract int GetLatestResourceRevision(string resourceID);
		public abstract int GetLatestStatementRevision(string statementID);
		public abstract Resource GetResourceByKey(Key key);
		public abstract Statement GetStatementByKey(Key key);
	}
}
