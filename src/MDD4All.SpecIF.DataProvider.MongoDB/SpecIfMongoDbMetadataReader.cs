/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{

	public class SpecIfMongoDbMetadataReader : AbstractSpecIfMetadataReader
	{
		private const string DATABASE_NAME = "specif";

		private MongoDBDataAccessor<ResourceClass> _resourceClassMongoDbAccessor;
		private MongoDBDataAccessor<PropertyClass> _propertyClassMongoDbAccessor;
		private MongoDBDataAccessor<StatementClass> _statementClassMongoDbAccessor;

		private MongoDBDataAccessor<DataType> _dataTypeMongoDbAccessor;

		public SpecIfMongoDbMetadataReader(string connectionString)
		{
			_resourceClassMongoDbAccessor = new MongoDBDataAccessor<ResourceClass>(connectionString, DATABASE_NAME);
			_propertyClassMongoDbAccessor = new MongoDBDataAccessor<PropertyClass>(connectionString, DATABASE_NAME);
			_dataTypeMongoDbAccessor = new MongoDBDataAccessor<DataType>(connectionString, DATABASE_NAME);
		}

		public override List<DataType> GetAllDataTypes()
		{
			return new List<DataType>(_dataTypeMongoDbAccessor.GetItems());
		}


		public override List<PropertyClass> GetAllPropertyClasses()
		{
			return new List<PropertyClass>(_propertyClassMongoDbAccessor.GetItems());
		}

		public override List<ResourceClass> GetAllResourceClasses()
		{
			return new List<ResourceClass>(_resourceClassMongoDbAccessor.GetItems());
		}

		public override DataType GetDataTypeById(string id)
		{
			return _dataTypeMongoDbAccessor.GetItemByFilter("{ id : '" + id + "' }");
		}

		public override string GetLatestPropertyClassRevision(string propertyClassID)
		{
			PropertyClass propertyClass = _propertyClassMongoDbAccessor.GetItemWithLatestRevision(propertyClassID);

			return propertyClass.Revision;
		}

		public override string GetLatestResourceClassRevision(string resourceClassID)
		{
			ResourceClass resourceClass = _resourceClassMongoDbAccessor.GetItemWithLatestRevision(resourceClassID);

			return resourceClass.Revision;
		}

		public override string GetLatestStatementClassRevision(string statementClassID)
		{
			StatementClass statementClass = _statementClassMongoDbAccessor.GetItemWithLatestRevision(statementClassID);

			return statementClass.Revision;
		}

		public override PropertyClass GetPropertyClassByKey(Key key)
		{
			PropertyClass result = null;

			if (key.Revision == Key.LATEST_REVISION)
			{
				string latestRevision = GetLatestPropertyClassRevision(key.ID);
				result = _propertyClassMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _propertyClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		public override ResourceClass GetResourceClassByKey(Key key)
		{
			ResourceClass result = null;

			if (key.Revision == Key.LATEST_REVISION)
			{
				string latestRevision = GetLatestResourceClassRevision(key.ID);
				result = _resourceClassMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _resourceClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}

		public override StatementClass GetStatementClassByKey(Key key)
		{
			StatementClass result = null;

			if (key.Revision == Key.LATEST_REVISION)
			{
				string latestRevision = GetLatestStatementClassRevision(key.ID);
				result = _statementClassMongoDbAccessor.GetItemById(key.ID + "_R_" + latestRevision);
			}
			else
			{
				result = _statementClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			}
			return result;
		}
	}
}
