/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Service;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public abstract class AbstractSpecIfMetadataReader : ISpecIfMetadataReader
	{
		public ISpecIfServiceDescription DataSourceDescription { get; set; }

		public List<EnumValue> GetEnumOptions(string dataTypeID)
		{
			List<EnumValue> result = new List<EnumValue>();

			DataType dataType = GetDataTypeById(dataTypeID);

			if (dataType != null && dataType.Values != null)
			{
				foreach (EnumValue value in dataType.Values)
				{
					result.Add(value);
				}
			}

			return result;
		}

		public List<string> GetDataTypeTypes()
		{
			List<string> result = new List<string>()
			{
				"xs:boolean", "xs:integer", "xs:double", "xs:dateTime", "xs:string", "xhtml" , "xs:enumeration"
			};

			return result;
		}

		public abstract List<DataType> GetAllDataTypes();
		public abstract List<PropertyClass> GetAllPropertyClasses();
		public abstract List<ResourceClass> GetAllResourceClasses();
		public abstract DataType GetDataTypeById(string id);
		public abstract PropertyClass GetPropertyClassByKey(Key key);
		public abstract ResourceClass GetResourceClassByKey(Key key);
		public abstract StatementClass GetStatementClassByKey(Key key);
		public abstract string GetLatestPropertyClassRevision(string propertyClassID);
		public abstract string GetLatestResourceClassRevision(string resourceClassID);
		public abstract string GetLatestStatementClassRevision(string statementClassID);
	}
}
