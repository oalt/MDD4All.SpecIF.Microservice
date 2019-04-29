/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbMetadataWriter : AbstractSpecIfMetadataWriter
	{
		private const string DATABASE_NAME = "specif";

		private MongoDBDataAccessor<DataType> _dataTypeMongoDbAccessor;
		private MongoDBDataAccessor<PropertyClass> _propertyClassMongoDbAccessor;
		private MongoDBDataAccessor<ResourceClass> _resourceClassMongoDbAccessor;
		private MongoDBDataAccessor<StatementClass> _statementClassMongoDbAccessor;

		public SpecIfMongoDbMetadataWriter(string connectionString)
		{
			_dataTypeMongoDbAccessor = new MongoDBDataAccessor<DataType>(connectionString, DATABASE_NAME);
			_propertyClassMongoDbAccessor = new MongoDBDataAccessor<PropertyClass>(connectionString, DATABASE_NAME);
			_resourceClassMongoDbAccessor = new MongoDBDataAccessor<ResourceClass>(connectionString, DATABASE_NAME);
			_statementClassMongoDbAccessor = new MongoDBDataAccessor<StatementClass>(connectionString, DATABASE_NAME);
		}

		public override void AddDataType(DataType dataType)
		{
			_dataTypeMongoDbAccessor.Add(dataType);
		}

		public override void UpdateDataType(DataType dataType)
		{
			_dataTypeMongoDbAccessor.Update(dataType, dataType.Id);
		}

		public override void AddPropertyClass(PropertyClass propertyClass)
		{
			_propertyClassMongoDbAccessor.Add(propertyClass);
		}

		public override void UpdatePropertyClass(PropertyClass propertyClass)
		{
			_propertyClassMongoDbAccessor.Update(propertyClass, propertyClass.Id);
		}

		public override void AddResourceClass(ResourceClass resourceClass)
		{
			_resourceClassMongoDbAccessor.Add(resourceClass);
		}

		public override void UpdateResourceClass(ResourceClass resourceClass)
		{
			_resourceClassMongoDbAccessor.Update(resourceClass, resourceClass.Id);
		}

		public override void AddStatementClass(StatementClass statementClass)
		{
			_statementClassMongoDbAccessor.Add(statementClass);
		}

		public override void UpdateStatementClass(StatementClass statementClass)
		{
			_statementClassMongoDbAccessor.Update(statementClass, statementClass.Id);
		}
	}
}
