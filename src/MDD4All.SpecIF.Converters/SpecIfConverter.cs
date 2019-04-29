/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.Converters
{
	public class SpecIfConverter
	{

		public void ConvertAll(DataModels.SpecIF sourceData, 
							   ISpecIfDataWriter targetDataWriter, 
							   ISpecIfMetadataWriter targetMetadataWriter,
							   bool overrideExistingData = false)
		{
			ConvertMetadata(sourceData, targetMetadataWriter);
			ConvertData(sourceData, targetDataWriter);
		}

		public void ConvertMetadata(DataModels.SpecIF sourceData, 
									ISpecIfMetadataWriter targetMetadataWriter,
									bool overrideExistingData = false)
		{
			if (sourceData.DataTypes != null)
			{
				foreach (DataType dataType in sourceData.DataTypes)
				{
					if (!overrideExistingData)
					{
						targetMetadataWriter.AddDataType(dataType);
					}
					else
					{
						targetMetadataWriter.UpdateDataType(dataType);
					}
				}
			}

			if(sourceData.PropertyClasses != null)
			{
				foreach (PropertyClass propertyClass in sourceData.PropertyClasses)
				{
					if (!overrideExistingData)
					{
						targetMetadataWriter.AddPropertyClass(propertyClass);
					}
					else
					{
						targetMetadataWriter.UpdatePropertyClass(propertyClass);
					}
				}
			}

			if (sourceData.ResourceClasses != null)
			{
				foreach (ResourceClass resourceClass in sourceData.ResourceClasses)
				{
					if (!overrideExistingData)
					{
						targetMetadataWriter.AddResourceClass(resourceClass);
					}
					else
					{
						targetMetadataWriter.UpdateResourceClass(resourceClass);
					}
				}
			}

			if (sourceData.StatementClasses != null)
			{
				foreach (StatementClass statementClass in sourceData.StatementClasses)
				{
					if (!overrideExistingData)
					{
						targetMetadataWriter.AddStatementClass(statementClass);
					}
					else
					{
						targetMetadataWriter.UpdateStatementClass(statementClass);
					}
				}
			}
		}

		public void ConvertData(DataModels.SpecIF sourceData, 
			                    ISpecIfDataWriter targetDataWriter,
								bool overrideExistingData = false)
		{
			if (sourceData.Resources != null)
			{
				foreach (Resource resource in sourceData.Resources)
				{
					if (!overrideExistingData)
					{
						targetDataWriter.AddResource(resource);
					}
					else
					{
						targetDataWriter.UpdateResource(resource);
					}
				}
			}

			if (sourceData.Statements != null)
			{
				foreach (Statement statement in sourceData.Statements)
				{
					if (!overrideExistingData)
					{
						targetDataWriter.AddStatement(statement);
					}
					else
					{
						targetDataWriter.UpdateStatement(statement);
					}
				}
			}

			if(sourceData.Hierarchies != null)
			{
				foreach (Node data in sourceData.Hierarchies)
				{
					data.IsHierarchyRoot = true;
					targetDataWriter.AddHierarchy(data);
					foreach (Node node in data.Nodes)
					{
						ConvertHierarchyNodesRecusrively(node, targetDataWriter, overrideExistingData);
					}
				}
			}


			//if (sourceData.Files != null)
			//{
			//	foreach (File file in sourceData.Files)
			//	{
			//		if (!overrideExistingData)
			//		{
			//			targetDataWriter.AddFile(file);
			//		}
			//		else
			//		{
			//			targetDataWriter.UpdateFile(file);
			//		}
			//	}
			//}
		}

		private void ConvertHierarchyNodesRecusrively(Node node, ISpecIfDataWriter dataWriter, bool overrideExistingData)
		{
			if (!overrideExistingData)
			{
				dataWriter.AddNode(node);
			}
			else
			{
				dataWriter.UpdateNode(node);
			}

			foreach (Node childNode in node.Nodes)
			{
				ConvertHierarchyNodesRecusrively(childNode, dataWriter, overrideExistingData);
			}

		}

	}
}
