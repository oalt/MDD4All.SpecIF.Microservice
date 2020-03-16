/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using System;

namespace MDD4All.MongoDB.DataAccess.Generic
{
	public class MongoDBDataAccessor<T>
	{
		private MongoClient _client;
		private IMongoDatabase _db;

		private string _collectionName;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="connectionString">The MongoDB connection string.</param>
		/// <param name="databaseName">The database name.</param>
		public MongoDBDataAccessor(string connectionString, string databaseName)
		{
			_client = new MongoClient(connectionString);
			_db = _client.GetDatabase(databaseName);

			string typeName = typeof(T).Name.ToLower();
			if(typeName.EndsWith("y"))
			{
				_collectionName = typeName.Substring(0, typeName.Length - 1) + "ies";
			}
			else if(typeName.EndsWith("s"))
			{
				_collectionName = typeName + "es";
			}
			else
			{
				_collectionName = typeName + "s";
			}
			
		}

		public IEnumerable<T> GetItems()
		{
			return _db.GetCollection<T>(_collectionName).Find(new BsonDocument()).ToEnumerable();
		}

        public T GetItemById(string id)
        {
            T result = default(T);

            BsonDocument filter = BsonDocument.Parse("{ \"_id\" : " + " \"" + id + "\"}");

            try
            {
                result = _db.GetCollection<T>(_collectionName).Find(filter).Single<T>();
            }
            catch
            {
            }

            return result;
        }

		public T GetItemByFilter(string filterJson)
		{
			T result = default(T);
			try
			{
				BsonDocument filter = BsonDocument.Parse(filterJson);

				result = _db.GetCollection<T>(_collectionName).Find(filter).Single<T>();
			}
			catch(Exception exception)
			{
				
			}
			return result;
		}

		public List<T> GetItemsByFilter(string filterJson)
		{
			List<T> result = new List<T>();
			try
			{
				BsonDocument filter = BsonDocument.Parse(filterJson);

				result = _db.GetCollection<T>(_collectionName).Find(filter).ToList();
			}
			catch (Exception exception)
			{

			}
			return result;
		}

		public T GetItemWithLatestRevision(string id)
		{
            T result = default(T);

            BsonDocument filter = new BsonDocument()
			{
				{"id", id },

			};

			//List<T> allRevisionItems = _db.GetCollection<T>(_collectionName).Find(filter).ToList();

			IFindFluent<T, T> allRevisions = _db.GetCollection<T>(_collectionName).Find(filter);

            if (allRevisions.CountDocuments() > 0)
            {
                result = allRevisions.Sort("{ 'revision.revisionNumber': -1}").First<T>();
            }
            return result;
		}

        public T GetItemWithLatestRevisionInBranch(string id, string branch = "main")
        {
            T result = default(T);

            BsonDocument filter = new BsonDocument()
            {
                { "id", id },
                { "revision.branch", branch }
            };

            IFindFluent<T, T> allRevisions = _db.GetCollection<T>(_collectionName).Find(filter);

            if (allRevisions.CountDocuments() > 0)
            {
                result = allRevisions.Sort("{ 'revision.revisionNumber': -1}").First<T>();
            }

            return result;
        }

        

		public void Add(T item)
		{
			try
			{
				IMongoCollection<T> collection = _db.GetCollection<T>(_collectionName);
				collection.InsertOne(item);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		public void Update(T item, string id)
		{
			try
			{
				IMongoCollection<T> collection = _db.GetCollection<T>(_collectionName);

				BsonDocument filter = BsonDocument.Parse("{ \"_id\" : " + " \"" + id + "\"}");

				ReplaceOneResult result = collection.ReplaceOne(filter, item, new UpdateOptions() { IsUpsert = true });
			}
			catch (Exception ex)
			{

			}
		}

        public bool Delete(string id)
        {

            bool result = false;
            try
            {
                IMongoCollection<T> collection = _db.GetCollection<T>(_collectionName);

                BsonDocument filter = BsonDocument.Parse("{ \"_id\" : " + " \"" + id + "\"}");

                DeleteResult deleteResult = collection.DeleteOne(filter);

                if(deleteResult.IsAcknowledged)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }
	}
}
