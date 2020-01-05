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
        public abstract List<Node> GetAllHierarchyRootNodes();
        public abstract List<Resource> GetAllResourceRevisions(string resourceID);
        public abstract List<Statement> GetAllStatementRevisions(string statementID);
        public abstract List<Statement> GetAllStatements();
        public abstract List<Statement> GetAllStatementsForResource(Key resourceKey);
        public abstract List<Node> GetChildNodes(Key parentNodeKey);
        public abstract List<Node> GetContainingHierarchyRoots(Key resourceKey);
		public abstract byte[] GetFile(string filename);
		public abstract Node GetHierarchyByKey(Key key);
		public abstract Revision GetLatestHierarchyRevision(string hierarchyID);
		public abstract Revision GetLatestResourceRevisionForBranch(string resourceID, string branchName);
		public abstract Revision GetLatestStatementRevision(string statementID);
		public abstract Resource GetResourceByKey(Key key);
		public abstract Statement GetStatementByKey(Key key);
	}
}
