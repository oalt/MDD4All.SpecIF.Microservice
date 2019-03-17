/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbDataReader : AbstractSpecIfDataReader
	{
		private const string DATABASE_NAME = "specif";

		private MongoDBDataAccessor<Resource> _resourceMongoDbAccessor;
		private MongoDBDataAccessor<Statement> _statementMongoDbAccessor;
		private MongoDBDataAccessor<Hierarchy> _hierarchyMongoDbAccessor;
		private MongoDBDataAccessor<Node> _nodeMongoDbAccessor;

		public SpecIfMongoDbDataReader(string connectionString)
		{
			_resourceMongoDbAccessor = new MongoDBDataAccessor<Resource>(connectionString, DATABASE_NAME);

			_statementMongoDbAccessor = new MongoDBDataAccessor<Statement>(connectionString, DATABASE_NAME);
			_hierarchyMongoDbAccessor = new MongoDBDataAccessor<Hierarchy>(connectionString, DATABASE_NAME);

			_nodeMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);			

		}

		public override List<Hierarchy> GetAllHierarchies()
		{
			return new List<Hierarchy>(_hierarchyMongoDbAccessor.GetItems());
		}

		public override byte[] GetFile(string filename)
		{
			// TODO Use mongodb for files
			string path = @"C:\specif\files_and_images\" + filename;

			byte[] result = System.IO.File.ReadAllBytes(path);

			return result;
		}

		public override Hierarchy GetHierarchyByKey(Key key)
		{
			Hierarchy result = null;

			if (key.Revision == 0)
			{
				int latestRevision = GetLatestHierarchyRevision(key.ID);
				result = _hierarchyMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _hierarchyMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		private void ResolveNodesRecursively(Node parent)
		{
			foreach (Key nodeKey in parent.NodeReferences)
			{
				if (parent.Nodes == null)
				{
					parent.Nodes = new List<Node>();
				}

				Node childNode = _nodeMongoDbAccessor.GetItemById(nodeKey.ID + "_R_" + nodeKey.Revision);

				parent.Nodes.Add(childNode);

				ResolveNodesRecursively(childNode);
			}

		}

		public override int GetLatestResourceRevision(string resourceID)
		{
			Resource resource = _resourceMongoDbAccessor.GetItemWithLatestRevision(resourceID);

			return resource.Revision;
		}

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			if (key.Revision == 0)
			{
				int latestRevision = GetLatestResourceRevision(key.ID);
				result = _resourceMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _resourceMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		public override Statement GetStatementByKey(Key key)
		{
			Statement result = null;

			if (key.Revision == 0)
			{
				int latestRevision = GetLatestStatementRevision(key.ID);
				result = _statementMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _statementMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		public override int GetLatestHierarchyRevision(string hierarchyID)
		{
			Hierarchy hierarchy = _hierarchyMongoDbAccessor.GetItemWithLatestRevision(hierarchyID);

			return hierarchy.Revision;
		}

		public override int GetLatestStatementRevision(string statementID)
		{
			Statement statement = _statementMongoDbAccessor.GetItemWithLatestRevision(statementID);

			return statement.Revision;
				 
		}
	}
}
