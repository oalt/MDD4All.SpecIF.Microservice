/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileDataWriter : AbstractSpecIfDataWriter
	{
		private DataModels.SpecIF _specIfData;

		private string _identificatorFilePath = @"c:\specif\identificators.json";

		private string _path;

		public SpecIfFileDataWriter(string path, ISpecIfMetadataReader metadataReader) : base(metadataReader)
		{
			_path = path;
			if (path == null)
			{
				_specIfData = new DataModels.SpecIF();
			}
			else
			{
				_specIfData = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(path);
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
			_specIfData?.Statements.Add(statement);
			SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
		}

		public override void AddResource(Resource resource)
		{
			_specIfData?.Resources.Add(resource);
			SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
		}

		public override void AddNode(Node newNode)
		{
			throw new NotImplementedException();
		}

		public override void UpdateNode(Node nodeToUpdate)
		{
			throw new NotImplementedException();
		}

		public override void AddHierarchy(Hierarchy hierarchy)
		{
			throw new NotImplementedException();
		}

		public override void UpdateHierarchy(Hierarchy hierarchyToUpdate)
		{
			throw new NotImplementedException();
		}

		public override void UpdateResource(Resource resource)
		{
			throw new NotImplementedException();
		}

		public void SaveHierarchy(Hierarchy hierarchy)
		{
			string id = hierarchy.ID;

			int index = -1;
			for (int counter = 0; counter < _specIfData.Hierarchies.Count; counter++)
			{
				if (_specIfData?.Hierarchies[counter].ID == id)
				{
					index = counter;
					break;
				}
			}

			if (index != -1)
			{
				_specIfData.Hierarchies[index] = hierarchy;
				SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
			}
			else
			{
				// new hierarchy
				_specIfData.Hierarchies.Add(hierarchy);
				SpecIfFileReaderWriter.SaveSpecIfToFile(_specIfData, _path);
			}

		}
	}
}
