/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using Newtonsoft.Json;
using System.IO;

namespace MDD4All.SpecIF.Converters
{
	public class FileToMongoDbConverter
	{
		private DataModels.SpecIF _specIF;
		private string _connectionString;

		private MongoDBDataAccessor<DataModels.Node> _nodeAccessor;
		

		public FileToMongoDbConverter(string filename, string connectionString)
		{
			_specIF = ReadDataFromSpecIfFile(filename);
			_connectionString = connectionString;

			_nodeAccessor = new MongoDBDataAccessor<Node>(_connectionString, "specif");		
		}

		public void ConvertFileToDB()
		{
			if (_specIF.DataTypes != null)
			{
				MongoDBDataAccessor<DataModels.DataType> dataTypeAccessor = new MongoDBDataAccessor<DataModels.DataType>(_connectionString, "specif");

				foreach (DataModels.DataType dataType in _specIF.DataTypes)
				{
					dataTypeAccessor.Add(dataType);
				}
			}
			
			if(_specIF.PropertyClasses != null)
			{
				MongoDBDataAccessor<DataModels.PropertyClass> propertyClassAccessor = new MongoDBDataAccessor<DataModels.PropertyClass>(_connectionString, "specif");

				foreach (PropertyClass propertyClass in _specIF.PropertyClasses)
				{
					propertyClassAccessor.Add(propertyClass);
				}
			}

			if (_specIF.ResourceClasses != null)
			{
				MongoDBDataAccessor<DataModels.ResourceClass> resourceTypeAccessor = new MongoDBDataAccessor<DataModels.ResourceClass>(_connectionString, "specif");

				foreach (DataModels.ResourceClass resourceClass in _specIF.ResourceClasses)
				{
					resourceTypeAccessor.Add(resourceClass);

					
				}
			}

			//if (_specIF.HierarchyClasses != null)
			//{
			//	MongoDBDataAccessor<DataModels.HierarchyClass> hierarchyTypeAccessor = new MongoDBDataAccessor<DataModels.HierarchyClass>(_connectionString, "specif");

			//	foreach (DataModels.HierarchyClass hierarchyType in _specIF.HierarchyClasses)
			//	{
			//		hierarchyTypeAccessor.Add(hierarchyType);
			//	}
			//}

			if (_specIF.StatementClasses != null)
			{
				MongoDBDataAccessor<DataModels.StatementClass> statementClassAccessor = new MongoDBDataAccessor<DataModels.StatementClass>(_connectionString, "specif");

				foreach (DataModels.StatementClass statementClass in _specIF.StatementClasses)
				{
					statementClassAccessor.Add(statementClass);
				}
			}

			if (_specIF.Resources != null)
			{
				MongoDBDataAccessor<DataModels.Resource> resourceAccessor = new MongoDBDataAccessor<DataModels.Resource>(_connectionString, "specif");

				foreach (DataModels.Resource resource in _specIF.Resources)
				{
					resourceAccessor.Add(resource);
				}
			}

			if (_specIF.Hierarchies != null)
			{
				MongoDBDataAccessor<DataModels.Node> resourceAccessor = new MongoDBDataAccessor<DataModels.Node>(_connectionString, "specif");

				foreach (DataModels.Node data in _specIF.Hierarchies)
				{
					resourceAccessor.Add(data);
					foreach(Node node in data.Nodes)
					{
						AddHierarchyNodesRecusrively(node);
					}
				}
			}

			if (_specIF.Statements != null)
			{
				MongoDBDataAccessor<DataModels.Statement> statementAccessor = new MongoDBDataAccessor<DataModels.Statement>(_connectionString, "specif");

				foreach (DataModels.Statement data in _specIF.Statements)
				{
					statementAccessor.Add(data);
					
				}
			}

			if(_specIF.Files != null)
			{
				MongoDBDataAccessor<DataModels.File> fileAccessor = new MongoDBDataAccessor<DataModels.File>(_connectionString, "specif");

				foreach (DataModels.File data in _specIF.Files)
				{
					fileAccessor.Add(data);
				}
			}
		}

		

		private void AddHierarchyNodesRecusrively(Node node)
		{
			_nodeAccessor.Add(node);
			foreach(Node childNode in node.Nodes)
			{
				AddHierarchyNodesRecusrively(childNode);
			}

		}

		private DataModels.SpecIF ReadDataFromSpecIfFile(string path)
		{
			DataModels.SpecIF result;

			StreamReader file = new StreamReader(path);

			JsonSerializer serializer = new JsonSerializer();

			result = (DataModels.SpecIF)serializer.Deserialize(file, typeof(DataModels.SpecIF));

			file.Close();

			return result;
		}
	}
}
