/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using Newtonsoft.Json;
using System;
using System.IO;

namespace MDD4All.SpecIF.Converters
{
	public class FileToMongoDbConverter
	{
		private string _filename = "";
		private DataModels.SpecIF _specIF;
		private string _connectionString;

		private MongoDBDataAccessor<DataModels.Node> _nodeAccessor;
		

		public FileToMongoDbConverter(string filename, string connectionString = "mongodb://localhost:27017")
		{
			_filename = filename;
			_connectionString = connectionString;

			_nodeAccessor = new MongoDBDataAccessor<Node>(_connectionString, "specif");		
		}

		/// <summary>
		/// Call this constructor, if you still have SpecIF data in memory.
		/// </summary>
		/// <param name="specIF">The data to write to the MongoDB.</param>
		public FileToMongoDbConverter(DataModels.SpecIF specIF)
		{
			_specIF = specIF;
		}

		/// <summary>
		/// Start the conversion process.
		/// </summary>
		public void ConvertFileToDB(bool overrideExistingData = false)
		{
			_specIF = ReadDataFromSpecIfFile(_filename);

			if (_specIF != null)
			{
				if (_specIF.DataTypes != null)
				{
					MongoDBDataAccessor<DataModels.DataType> dataTypeAccessor = new MongoDBDataAccessor<DataModels.DataType>(_connectionString, "specif");

					foreach (DataModels.DataType dataType in _specIF.DataTypes)
					{
						if (!overrideExistingData)
						{
							dataTypeAccessor.Add(dataType);
						}
						else
						{
							dataTypeAccessor.Update(dataType, dataType.Id);
						}
					}
				}

				if (_specIF.PropertyClasses != null)
				{
					MongoDBDataAccessor<DataModels.PropertyClass> propertyClassAccessor = new MongoDBDataAccessor<DataModels.PropertyClass>(_connectionString, "specif");

					foreach (PropertyClass propertyClass in _specIF.PropertyClasses)
					{
						if (!overrideExistingData)
						{
							propertyClassAccessor.Add(propertyClass);
						}
						else
						{
							propertyClassAccessor.Update(propertyClass, propertyClass.Id);
						}
					}
				}

				if (_specIF.ResourceClasses != null)
				{
					MongoDBDataAccessor<DataModels.ResourceClass> resourceClassAccessor = new MongoDBDataAccessor<DataModels.ResourceClass>(_connectionString, "specif");

					foreach (DataModels.ResourceClass resourceClass in _specIF.ResourceClasses)
					{
						if (!overrideExistingData)
						{
							resourceClassAccessor.Add(resourceClass);
						}
						else
						{
							resourceClassAccessor.Update(resourceClass, resourceClass.Id);
						}



					}
				}

				if (_specIF.StatementClasses != null)
				{
					MongoDBDataAccessor<DataModels.StatementClass> statementClassAccessor = new MongoDBDataAccessor<DataModels.StatementClass>(_connectionString, "specif");

					foreach (DataModels.StatementClass statementClass in _specIF.StatementClasses)
					{
						if (!overrideExistingData)
						{
							statementClassAccessor.Add(statementClass);
						}
						else
						{
							statementClassAccessor.Update(statementClass, statementClass.Id);
						}
					}
				}

				if (_specIF.Resources != null)
				{
					MongoDBDataAccessor<DataModels.Resource> resourceAccessor = new MongoDBDataAccessor<DataModels.Resource>(_connectionString, "specif");

					foreach (DataModels.Resource resource in _specIF.Resources)
					{
						if (!overrideExistingData)
						{
							resourceAccessor.Add(resource);
						}
						else
						{
							resourceAccessor.Update(resource, resource.Id);
						}
					}
				}

				if (_specIF.Hierarchies != null)
				{
					MongoDBDataAccessor<DataModels.Node> nodeAccessor = new MongoDBDataAccessor<DataModels.Node>(_connectionString, "specif");

					foreach (Node data in _specIF.Hierarchies)
					{
						data.IsHierarchyRoot = true;
						nodeAccessor.Add(data);
						foreach (Node node in data.Nodes)
						{
							AddHierarchyNodesRecusrively(node, overrideExistingData);
						}
					}
				}

				if (_specIF.Statements != null)
				{
					MongoDBDataAccessor<DataModels.Statement> statementAccessor = new MongoDBDataAccessor<DataModels.Statement>(_connectionString, "specif");

					foreach (DataModels.Statement data in _specIF.Statements)
					{
						if (!overrideExistingData)
						{
							statementAccessor.Add(data);
						}
						else
						{
							statementAccessor.Update(data, data.Id);
						}
					}
				}

				if (_specIF.Files != null)
				{
					MongoDBDataAccessor<DataModels.File> fileAccessor = new MongoDBDataAccessor<DataModels.File>(_connectionString, "specif");

					foreach (DataModels.File data in _specIF.Files)
					{
						if (!overrideExistingData)
						{
							fileAccessor.Add(data);
						}
						else
						{
							fileAccessor.Update(data, data.Id);
						}
					}
				}
			}
		}

		

		private void AddHierarchyNodesRecusrively(Node node, bool overrideExistingData)
		{
			if (!overrideExistingData)
			{
				_nodeAccessor.Add(node);
			}
			else
			{
				_nodeAccessor.Update(node, node.Id);
			}

			foreach(Node childNode in node.Nodes)
			{
				AddHierarchyNodesRecusrively(childNode, overrideExistingData);
			}

		}

		private DataModels.SpecIF ReadDataFromSpecIfFile(string path)
		{
			DataModels.SpecIF result = null;

			try
			{
				StreamReader file = new StreamReader(path);

				JsonSerializer serializer = new JsonSerializer();

				result = (DataModels.SpecIF)serializer.Deserialize(file, typeof(DataModels.SpecIF));

				file.Close();
			}
			catch(Exception)
			{

			}

			return result;
		}
	}
}
