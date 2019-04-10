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
		public abstract string GetLatestHierarchyRevision(string hierarchyID);
		public abstract string GetLatestResourceRevision(string resourceID);
		public abstract string GetLatestStatementRevision(string statementID);
		public abstract Resource GetResourceByKey(Key key);
		public abstract Statement GetStatementByKey(Key key);
	}
}
