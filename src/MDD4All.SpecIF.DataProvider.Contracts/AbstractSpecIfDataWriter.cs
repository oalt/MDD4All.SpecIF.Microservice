/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Linq;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
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
			result.Revision = Key.FIRST_MAIN_REVISION;
			result.Properties = new List<Property>();

			result.Class = resourceTypeID;

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

        public SpecIfBaseElement UpdateVersionInfo<T>(SpecIfBaseElement data)
        {
            SpecIfBaseElement result = data;

            string targetBranchName = "";

            // set revision info
            if (string.IsNullOrEmpty(data.ID)) // no id given. Create new id and add element as main/1
            {
                data.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
                data.Revision = Key.FIRST_MAIN_REVISION;
                data.Replaces = new List<Revision>();

            }
            else // id given
            {
                if (data.Revision == null) // no revision set. Set default targetbranch == "main"
                {
                    data.Revision = new Revision();
                    targetBranchName = "main";
                }
                else
                {

                    if (!string.IsNullOrEmpty(data.Revision.BranchName)) // target branch given by revision. Set as target branch
                    {
                        targetBranchName = data.Revision.BranchName;
                    }
                    else
                    {
                        targetBranchName = "main";
                    }
                }
            }

            IdentifiableElement parentElement = GetItemWithLatestRevisionInBranch<T>(data.ID, targetBranchName);

            if (parentElement != null)
            {
                data.Revision.RevsionNumber = parentElement.Revision.RevsionNumber + 1;
                data.Revision.BranchName = parentElement.Revision.BranchName;
            }
            else
            {
                data.Revision.RevsionNumber = 1;
                data.Revision.BranchName = targetBranchName;
            }

            // set replacement info
            if (data.Replaces.Count == 0) // no replace given. Take latest of target branch as replacement
            { 
                if (parentElement == null) // branch root element
                {
                    data.Replaces = new List<Revision>();
                }
                else
                {
                    data.Replaces = new List<Revision>();
                    data.Replaces.Add(parentElement.Revision);
                }
            }

            result.ChangedAt = DateTime.Now;

            return result;
        }

        public abstract void InitializeIdentificators();
        public abstract void SaveIdentificators();
        public abstract Node SaveHierarchy(Node hierarchyToUpdate);
        public abstract Node SaveNode(Node nodeToUpdate);
        public abstract Resource SaveResource(Resource resource);
        public abstract Statement SaveStatement(Statement statement);

        public abstract void AddHierarchy(Node hierarchy);
        public abstract void AddResource(Resource resource);
        public abstract void AddStatement(Statement statement);
        protected abstract IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch);
        public abstract void AddNode(string parentNodeID, Node newNode);
    }
}
