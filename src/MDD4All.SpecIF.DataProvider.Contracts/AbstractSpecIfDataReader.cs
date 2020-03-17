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
        public abstract List<Node> GetAllHierarchyRootNodes(string projectID = null);
        public abstract List<Resource> GetAllResourceRevisions(string resourceID);
        public abstract List<Statement> GetAllStatementRevisions(string statementID);
        public abstract List<Statement> GetAllStatements();
        public abstract List<Statement> GetAllStatementsForResource(Key resourceKey);
        public abstract List<Node> GetChildNodes(Key parentNodeKey);
        public abstract List<Node> GetContainingHierarchyRoots(Key resourceKey);
		public abstract byte[] GetFile(string filename);
		public abstract Node GetHierarchyByKey(Key key);
		public abstract string GetLatestHierarchyRevision(string hierarchyID);
		public abstract string GetLatestResourceRevisionForBranch(string resourceID, string branchName);
		public abstract string GetLatestStatementRevision(string statementID);
        public abstract Node GetNodeByKey(Key key);
        public abstract Node GetParentNode(Key childNode);
        public abstract SpecIF.DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, string projectID, List<Key> hierarchyFilter = null, bool includeMetadata = true);
        public abstract List<ProjectDescriptor> GetProjectDescriptions();
        public abstract Resource GetResourceByKey(Key key);
		public abstract Statement GetStatementByKey(Key key);
	}
}
