/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfMetadataWriter
	{
		void AddDataType(DataType dataType);

		void UpdateDataType(DataType dataType);

		void AddPropertyClass(PropertyClass propertyClass);

		void UpdatePropertyClass(PropertyClass propertyClass);

		void AddResourceClass(ResourceClass resourceClass);

		void UpdateResourceClass(ResourceClass resourceClass);

		void AddStatementClass(StatementClass statementClass);

		void UpdateStatementClass(StatementClass statementClass);
	}
}
