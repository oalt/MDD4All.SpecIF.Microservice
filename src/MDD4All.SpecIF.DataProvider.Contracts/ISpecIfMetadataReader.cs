/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfMetadataReader : ISpecIfDataProviderBase
	{
		List<DataType> GetAllDataTypes();

		DataType GetDataTypeById(string id);

		List<string> GetDataTypeTypes();

		List<EnumValue> GetEnumOptions(string dataTypeID);

		List<PropertyClass> GetAllPropertyClasses();

		PropertyClass GetPropertyClassByKey(Key key);

		string GetLatestPropertyClassRevision(string propertyClassID);

		List<ResourceClass> GetAllResourceClasses();

		ResourceClass GetResourceClassByKey(Key key);

		string GetLatestResourceClassRevision(string resourceClassID);

		StatementClass GetStatementClassByKey(Key key);

		string GetLatestStatementClassRevision(string statementClassID);
	}
}
