/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Linq;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;

namespace MDD4All.SpecIF.DataProvider.Contracts
{
	public abstract class AbstractSpecIfDataWriter : ISpecIfDataWriter
	{
		protected SpecIfIdentifiers _identificators;

		protected ISpecIfMetadataReader _metadataReader;

		public ISpecIfServiceDescription DataSourceDescription { get; set; }

		public AbstractSpecIfDataWriter(ISpecIfMetadataReader metadataReader)
		{
			_metadataReader = metadataReader;
		}

		public Resource CreateResource(Key resourceTypeID)
		{
			Resource result = new Resource();

			ResourceClass resourceType = _metadataReader.GetResourceClassByKey(resourceTypeID);

			result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
			result.Revision = 1;
			result.Properties = new List<Property>();

			result.ResourceClass = resourceTypeID;

			foreach (Key propertyClassReference in resourceType.PropertyClasses)
			{

				PropertyClass propertyClass = _metadataReader.GetPropertyClassByKey(propertyClassReference);

				Property property = new Property()
				{
					ID = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
					Title = propertyClass.Title,
					PropertyClass = propertyClassReference,
					Description = propertyClass.Description
				};

				result.Properties.Add(property);
			}

			result.ChangedAt = DateTime.Now;
			// TODO changeBy implementation
			result.ChangedBy = "";

			return result;
		}

		public Hierarchy CreateHierarchy(Key hierarchyClassKey)
		{
			Hierarchy result = new Hierarchy();

			HierarchyClass hierarchyType = _metadataReader.GetHierarchyClassByKey(hierarchyClassKey);

			result.HierarchyClass = hierarchyClassKey;

			result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
			result.Revision = 1;

			result.Nodes = new List<Node>();

			result.Properties = new List<Property>();



			foreach (Key propertyClassReference in hierarchyType.PropertyClasses)
			{
				PropertyClass propertyClass = _metadataReader.GetPropertyClassByKey(propertyClassReference);

				Property property = new Property()
				{
					ID = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
					Title = propertyClass.Title,
					PropertyClass = propertyClassReference,
					Description = propertyClass.Description
				};

				result.Properties.Add(property);
			}


			// TODO changeBy implementation
			result.ChangedBy = "";


			return result;
		}



		public long GetNextSpecIfIdentifier(string prefix)
		{
			long result = 0;

			if (_identificators == null)
			{
				_identificators = new SpecIfIdentifiers();
			}

			SpecIfIdentifier identifier = _identificators.Identifiers.FirstOrDefault(identificator => identificator.Prefix == prefix);

			if (identifier != null)
			{
				identifier.Number = identifier.Number + 1;
				result = identifier.Number;
			}
			else
			{
				SpecIfIdentifier newIdentifier = new SpecIfIdentifier()
				{
					Prefix = prefix,
					Number = 1
				};
				_identificators.Identifiers.Add(newIdentifier);
				result = 1;
			}

			SaveIdentificators();

			return result;
		}

		public abstract void AddHierarchy(Hierarchy hierarchy);
		public abstract void AddNode(Node newNode);
		public abstract void AddResource(Resource resource);
		public abstract void AddStatement(Statement statement);
		public abstract void InitializeIdentificators();
		public abstract void SaveIdentificators();
		public abstract void UpdateHierarchy(Hierarchy hierarchyToUpdate);
		public abstract void UpdateNode(Node nodeToUpdate);
		public abstract void UpdateResource(Resource resource);
	}
}
