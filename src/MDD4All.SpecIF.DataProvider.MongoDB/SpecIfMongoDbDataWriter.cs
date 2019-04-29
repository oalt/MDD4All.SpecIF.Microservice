/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbDataWriter : AbstractSpecIfDataWriter
	{
		private const string DATABASE_NAME = "specif";

		private const string SPECIF_ADMIN_DATABASE_NAME = "specifAdmin";

		private MongoDBDataAccessor<Resource> _resourceMongoDbAccessor;
		private MongoDBDataAccessor<Statement> _statementMongoDbAccessor;
		private MongoDBDataAccessor<Node> _hierarchyMongoDbAccessor;
		private MongoDBDataAccessor<Node> _nodeMongoDbAccessor;

		private MongoDBDataAccessor<SpecIfIdentifier> _identifierMongoDbAccessor;

		public SpecIfMongoDbDataWriter(string connectionString, ISpecIfMetadataReader metadataReader) : base(metadataReader)
		{
			_resourceMongoDbAccessor = new MongoDBDataAccessor<Resource>(connectionString, DATABASE_NAME);

			_statementMongoDbAccessor = new MongoDBDataAccessor<Statement>(connectionString, DATABASE_NAME);
			_hierarchyMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_nodeMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_identifierMongoDbAccessor = new MongoDBDataAccessor<SpecIfIdentifier>(connectionString, SPECIF_ADMIN_DATABASE_NAME);

			InitializeIdentificators();
		}

		public override void AddNode(Node newNode)
		{
			_nodeMongoDbAccessor.Add(newNode);
		}

		public override void UpdateNode(Node nodeToUpdate)
		{
			_nodeMongoDbAccessor.Update(nodeToUpdate, nodeToUpdate.Id);
		}

		public override void AddResource(Resource resource)
		{
			_resourceMongoDbAccessor.Add(resource);
		}

		public override void AddStatement(Statement statement)
		{
			_statementMongoDbAccessor.Add(statement);
		}

		public override void InitializeIdentificators()
		{
			SpecIfIdentifiers identifiers = new SpecIfIdentifiers();

			identifiers.Identifiers = new List<SpecIfIdentifier>(_identifierMongoDbAccessor.GetItems());

			_identificators = identifiers;
		}

		public override void SaveIdentificators()
		{
			foreach (SpecIfIdentifier identifier in _identificators.Identifiers)
			{
				_identifierMongoDbAccessor.Update(identifier, identifier.Id);
			}
		}

		public override void AddHierarchy(Node hierarchy)
		{
			_hierarchyMongoDbAccessor.Add(hierarchy);
		}

		public override void UpdateHierarchy(Node hierarchyToUpdate)
		{
			_hierarchyMongoDbAccessor.Update(hierarchyToUpdate, hierarchyToUpdate.Id);
		}

		public override void UpdateResource(Resource resource)
		{
			_resourceMongoDbAccessor.Update(resource, resource.Id);
		}

		public override void UpdateStatement(Statement statement)
		{
			_statementMongoDbAccessor.Update(statement, statement.Id);
		}
	}
}
