/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileMetadataReader : AbstractSpecIfMetadataReader
	{

		private DataModels.SpecIF _metaData;

		public SpecIfFileMetadataReader()
		{
			_metaData = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(@"c:\specif\metadata.specif");
		}

		public override List<DataType> GetAllDataTypes()
		{
			return _metaData?.DataTypes;
		}

		public override DataType GetDataTypeById(string id)
		{
			return _metaData?.DataTypes.FirstOrDefault(dataType => dataType.ID == id);
		}

		
		public override ResourceClass GetResourceClassByKey(Key key)
		{
			ResourceClass result = null;

			List<ResourceClass> resourceClassesWithSameID = _metaData?.ResourceClasses.FindAll(res => res.ID == key.ID);

			if (resourceClassesWithSameID.Count != 0)
			{
				if (key.Revision == Key.LATEST_REVISION)
				{
					result = resourceClassesWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = resourceClassesWithSameID.Find(r => r.Revision == key.Revision);
				}
			}

			return result;
		}

		public override PropertyClass GetPropertyClassByKey(Key key)
		{
			PropertyClass result = null;

			List<PropertyClass> propertyClassesWithSameID = _metaData?.PropertyClasses.FindAll(res => res.ID == key.ID);

			if (propertyClassesWithSameID.Count != 0)
			{
				if (key.Revision == Key.LATEST_REVISION)
				{
					result = propertyClassesWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = propertyClassesWithSameID.Find(r => r.Revision == key.Revision);
				}
			}

			return result;
		}

		public override List<PropertyClass> GetAllPropertyClasses()
		{
			return _metaData?.PropertyClasses;
		}

		public override StatementClass GetStatementClassByKey(Key key)
		{
			StatementClass result = null;

			List<StatementClass> statementClassesWithSameID = _metaData?.StatementClasses.FindAll(res => res.ID == key.ID);

			if (statementClassesWithSameID.Count != 0)
			{
				if (key.Revision == Key.LATEST_REVISION)
				{
					result = statementClassesWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = statementClassesWithSameID.Find(r => r.Revision == key.Revision);
				}
			}

			return result;
		}

		public override List<ResourceClass> GetAllResourceClasses()
		{
			return _metaData?.ResourceClasses;
		}

		public override Revision GetLatestPropertyClassRevision(string propertyClassID)
		{
			throw new System.NotImplementedException();
		}

		public override Revision GetLatestResourceClassRevision(string resourceClassID)
		{
			throw new System.NotImplementedException();
		}

		public override Revision GetLatestStatementClassRevision(string statementClassID)
		{
			throw new System.NotImplementedException();
		}

	
	}
}
