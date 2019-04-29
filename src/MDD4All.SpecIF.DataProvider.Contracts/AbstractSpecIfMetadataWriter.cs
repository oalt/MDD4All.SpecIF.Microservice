/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public abstract class AbstractSpecIfMetadataWriter : ISpecIfMetadataWriter
	{
		public abstract void AddDataType(DataType dataType);
		public abstract void AddPropertyClass(PropertyClass propertyClass);
		public abstract void AddResourceClass(ResourceClass resourceClass);
		public abstract void AddStatementClass(StatementClass statementClass);
		public abstract void UpdateDataType(DataType dataType);
		public abstract void UpdatePropertyClass(PropertyClass propertyClass);
		public abstract void UpdateResourceClass(ResourceClass resourceClass);
		public abstract void UpdateStatementClass(StatementClass statementClass);
	}
}
