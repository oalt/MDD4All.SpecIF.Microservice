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

		DataType GetDataTypeByKey(Key key);
        
        List<DataType> GetAllDataTypeRevisions(string dataTypeID);

        List<string> GetDataTypeTypes();

        List<EnumValue> GetEnumOptions(string dataTypeID);

		List<PropertyClass> GetAllPropertyClasses();

		PropertyClass GetPropertyClassByKey(Key key);

        List<PropertyClass> GetAllPropertyClassRevisions(string propertyClassID);

		string GetLatestPropertyClassRevision(string propertyClassID);

		List<ResourceClass> GetAllResourceClasses();

		ResourceClass GetResourceClassByKey(Key key);

        List<ResourceClass> GetAllResourceClassRevisions(string resourceClassID);

		string GetLatestResourceClassRevision(string resourceClassID);

        List<StatementClass> GetAllStatementClasses();

		StatementClass GetStatementClassByKey(Key key);

        List<StatementClass> GetAllStatementClassRevisions(string statementClassID);

        string GetLatestStatementClassRevision(string statementClassID);
	}
}
