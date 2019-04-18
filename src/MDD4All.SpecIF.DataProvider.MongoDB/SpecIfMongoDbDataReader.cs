/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbDataReader : AbstractSpecIfDataReader
	{
		private const string DATABASE_NAME = "specif";

		private MongoDBDataAccessor<Resource> _resourceMongoDbAccessor;
		private MongoDBDataAccessor<Statement> _statementMongoDbAccessor;
		private MongoDBDataAccessor<Node> _hierarchyMongoDbAccessor;
		private MongoDBDataAccessor<Node> _nodeMongoDbAccessor;

		public SpecIfMongoDbDataReader(string connectionString)
		{
			_resourceMongoDbAccessor = new MongoDBDataAccessor<Resource>(connectionString, DATABASE_NAME);

			_statementMongoDbAccessor = new MongoDBDataAccessor<Statement>(connectionString, DATABASE_NAME);
			_hierarchyMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_nodeMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);			

		}

		public override List<Node> GetAllHierarchies()
		{
			List<Node> result = new List<Node>();

			BsonDocument filter = new BsonDocument()
			{
				{  "isHierarchyRoot", true }
			};

			result = new List<Node>(_hierarchyMongoDbAccessor.GetItemsByFilter(filter.ToJson()));

			foreach(Node rootNode in result)
			{
				ResolveNodesRecursively(rootNode);
			}

			return result;
		}

		public override byte[] GetFile(string filename)
		{
			// TODO Use mongodb for files
			string path = @"C:\specif\files_and_images\" + filename;

			byte[] result = System.IO.File.ReadAllBytes(path);

			return result;
		}

		public override Node GetHierarchyByKey(Key key)
		{
			Node result = null;

			if (key.Revision == Key.LATEST_REVISION)
			{
				Revision latestRevision = GetLatestHierarchyRevision(key.ID);
				result = _hierarchyMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision.StringValue);
			}
			else
			{
				result = _hierarchyMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision.StringValue);
			}

			ResolveNodesRecursively(result);

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

				Node childNode = _nodeMongoDbAccessor.GetItemById(nodeKey.ID + "_R_" + nodeKey.Revision.StringValue);

				parent.Nodes.Add(childNode);

				ResolveNodesRecursively(childNode);
			}

		}

		public override Revision GetLatestResourceRevision(string resourceID)
		{
			Resource resource = _resourceMongoDbAccessor.GetItemWithLatestRevision(resourceID);

			return resource.Revision;
		}

		

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			if (key.Revision == Key.LATEST_REVISION)
			{
				Revision latestRevision = GetLatestResourceRevision(key.ID);
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

			if (key.Revision == Key.LATEST_REVISION)
			{
				Revision latestRevision = GetLatestStatementRevision(key.ID);
				result = _statementMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _statementMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		public override Revision GetLatestHierarchyRevision(string hierarchyID)
		{
			Node hierarchy = _hierarchyMongoDbAccessor.GetItemWithLatestRevision(hierarchyID);

			return hierarchy.Revision;
		}

		public override Revision GetLatestStatementRevision(string statementID)
		{
			Statement statement = _statementMongoDbAccessor.GetItemWithLatestRevision(statementID);

			return statement.Revision;
				 
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
			List<Statement> result = new List<Statement>();

			BsonDocument filter = new BsonDocument()
			{
				{ "$or", new BsonArray()
						 {
							new BsonDocument()
							{
								{ "subject.id", resourceKey.ID }
							},
							new BsonDocument()
							{
								{ "object.id", resourceKey.ID }
							}
						}
				}
			};

			result = _statementMongoDbAccessor.GetItemsByFilter(filter.ToJson());

			return result;
		}
	}
}
