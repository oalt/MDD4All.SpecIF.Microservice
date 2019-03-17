/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public interface ISpecIfDataWriter : ISpecIfDataProviderBase
	{
		Resource CreateResource(Key resourceClassKey);

		void AddResource(Resource resource);

		void UpdateResource(Resource resource);

		Hierarchy CreateHierarchy(Key hierarchyClassKey);

		void AddHierarchy(Hierarchy hierarchy);

		void UpdateHierarchy(Hierarchy hierarchyToUpdate);

		void AddStatement(Statement statement);

		void AddNode(Node newNode);

		void UpdateNode(Node nodeToUpdate);

		long GetNextSpecIfIdentifier(string prefix);

		void InitializeIdentificators();

		void SaveIdentificators();

	}
}
