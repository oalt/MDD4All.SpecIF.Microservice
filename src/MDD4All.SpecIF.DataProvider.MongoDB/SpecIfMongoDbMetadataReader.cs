/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MongoDB.Bson;
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
            _statementClassMongoDbAccessor = new MongoDBDataAccessor<StatementClass>(connectionString, DATABASE_NAME);
		}

        public override List<DataType> GetAllDataTypeRevisions(string dataTypeID)
        {
            List<DataType> result = new List<DataType>();

            BsonDocument filter = new BsonDocument()
            {
                {  "id", dataTypeID }
            };

            List<DataType> dbResult = _dataTypeMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<DataType> GetAllDataTypes()
		{
			return new List<DataType>(_dataTypeMongoDbAccessor.GetItems());
		}


		public override List<PropertyClass> GetAllPropertyClasses()
		{
			return new List<PropertyClass>(_propertyClassMongoDbAccessor.GetItems());
		}

        public override List<PropertyClass> GetAllPropertyClassRevisions(string propertyClassID)
        {
            List<PropertyClass> result = new List<PropertyClass>();

            BsonDocument filter = new BsonDocument()
            {
                {  "id", propertyClassID }
            };

            List<PropertyClass> dbResult = _propertyClassMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<ResourceClass> GetAllResourceClasses()
		{
			return new List<ResourceClass>(_resourceClassMongoDbAccessor.GetItems());
		}

        public override List<ResourceClass> GetAllResourceClassRevisions(string resourceClassID)
        {
            List<ResourceClass> result = new List<ResourceClass>();

            BsonDocument filter = new BsonDocument()
            {
                {  "id", resourceClassID }
            };

            List<ResourceClass> dbResult = _resourceClassMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<StatementClass> GetAllStatementClasses()
        {
            return new List<StatementClass>(_statementClassMongoDbAccessor.GetItems());
        }

        public override List<StatementClass> GetAllStatementClassRevisions(string statementClassID)
        {
            List<StatementClass> result = new List<StatementClass>();

            BsonDocument filter = new BsonDocument()
            {
                {  "id", statementClassID }
            };

            List<StatementClass> dbResult = _statementClassMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override DataType GetDataTypeByKey(Key key)
		{
            DataType result = null;

            
            result = _dataTypeMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
            
            return result;
		}

		public override Revision GetLatestPropertyClassRevision(string propertyClassID)
		{
            Revision result = null;

			PropertyClass propertyClass = _propertyClassMongoDbAccessor.GetItemWithLatestRevision(propertyClassID);

            if(propertyClass != null)
            {
                //result = propertyClass.Revision;
            }

			return result;
		}

		public override Revision GetLatestResourceClassRevision(string resourceClassID)
		{
            Revision result = null;

			ResourceClass resourceClass = _resourceClassMongoDbAccessor.GetItemWithLatestRevision(resourceClassID);

            if(resourceClass != null)
            {
                //result = resourceClass.Revision;
            }

			return result;
		}

		public override Revision GetLatestStatementClassRevision(string statementClassID)
		{
            Revision result = null;

			StatementClass statementClass = _statementClassMongoDbAccessor.GetItemWithLatestRevision(statementClassID);

            if(statementClass != null)
            {
                //result = statementClass.Revision;
            }

			return result;
		}

		public override PropertyClass GetPropertyClassByKey(Key key)
		{
			PropertyClass result = null;

			
			result = _propertyClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			
			return result;
		}

		public override ResourceClass GetResourceClassByKey(Key key)
		{
			ResourceClass result = null;

			
			result = _resourceClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			
			return result;
		}

		public override StatementClass GetStatementClassByKey(Key key)
		{
			StatementClass result = null;

            
            result = _statementClassMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
            

			return result;
		}
	}
}
