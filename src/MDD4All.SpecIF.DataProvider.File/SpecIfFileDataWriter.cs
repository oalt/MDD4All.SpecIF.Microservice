/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileDataWriter : AbstractSpecIfDataWriter
	{
		private Dictionary<string, DataModels.SpecIF> _specIfData;

		private string _identificatorFilePath = @"c:\specif\identificators.json";

		private string _path;

		public SpecIfFileDataWriter(string path, ISpecIfMetadataReader metadataReader, 
            ISpecIfDataReader dataReader) : base(metadataReader, dataReader)
		{
			_path = path;
			if (path == null)
			{
				_specIfData = new Dictionary<string, DataModels.SpecIF>();
			}
			else
			{
                if(dataReader is SpecIfFileDataReader)
                {
                    SpecIfFileDataReader fileDataReader = dataReader as SpecIfFileDataReader;
                    _specIfData = fileDataReader.SpecIfData;
                }
				
			}

			InitializeIdentificators();
		}

		public override void InitializeIdentificators()
		{
			FileInfo fileInfo = new FileInfo(_identificatorFilePath);
			if (fileInfo.Exists)
			{
				StreamReader file = new StreamReader(_identificatorFilePath);

				JsonSerializer serializer = new JsonSerializer();

				_identificators = (SpecIfIdentifiers)serializer.Deserialize(file, typeof(SpecIfIdentifiers));

				file.Close();
			}
			else
			{
				_identificators = new SpecIfIdentifiers();
				SaveIdentificators();
			}
		}

		public override void SaveIdentificators()
		{
			StreamWriter sw = new StreamWriter(_identificatorFilePath);
			JsonWriter writer = new JsonTextWriter(sw)
			{
				Formatting = Formatting.Indented
			};

			JsonSerializer serializer = new JsonSerializer()
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			serializer.Serialize(writer, _identificators);

			writer.Flush();
			writer.Close();
		}

        public override void AddStatement(Statement statement)
		{
			//_specIfData?.Statements.Add(statement);
			//SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
		}

        public override void AddResource(Resource resource)
		{
			//_specIfData?.Resources.Add(resource);
			//SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
		}

        public override void AddNode(string parentNodeId, Node newNode)
		{
			throw new NotImplementedException();
		}

        public override void AddHierarchy(Node hierarchy, string projectID = null)
		{
			//_specIfData?.Hierarchies.Add(hierarchy);
			//SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
		}

		public override Resource SaveResource(Resource resource, string projectID = null)
		{
			throw new NotImplementedException();
		}

		public override Node SaveHierarchy(Node hierarchy)
		{
            Node result = null;

			//string id = hierarchy.ID;

			//int index = -1;
			//for (int counter = 0; counter < _specIfData.Hierarchies.Count; counter++)
			//{
			//	if (_specIfData?.Hierarchies[counter].ID == id)
			//	{
			//		index = counter;
			//		break;
			//	}
			//}

			//if (index != -1)
			//{
			//	_specIfData.Hierarchies[index] = hierarchy;
			//	SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
			//}
			//else
			//{
			//	// new hierarchy
			//	_specIfData.Hierarchies.Add(hierarchy);
			//	SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
			//}

            return result;
		}

		public override Statement SaveStatement(Statement statement, string projectID = null)
		{


			//Statement existingStatement = _specIfData?.Statements.Find(st => st.ID == statement.ID && st.Revision == statement.Revision);

			//if(existingStatement != null)
			//{
			//	existingStatement = statement;
			//}
			//else
			//{
			//	AddStatement(statement);
			//}
			//SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);

            return statement;
		}

        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
        {
            throw new NotImplementedException();
        }

        public override void MoveNode(string nodeID, string newParentID, string newSiblingId)
        {
            throw new NotImplementedException();
        }

        public override Resource UpdateResource(Resource resource)
        {
            throw new NotImplementedException();
        }

        public override void AddProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project, string integrationID = null)
        {
            throw new NotImplementedException();
        }

        public override void UpdateProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project)
        {
            throw new NotImplementedException();
        }

        public override void DeleteProject(string projectID)
        {
            throw new NotImplementedException();
        }
    }
}
