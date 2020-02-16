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

        protected ISpecIfDataReader _dataReader;

		public ISpecIfServiceDescription DataSourceDescription { get; set; }

		public AbstractSpecIfDataWriter(ISpecIfMetadataReader metadataReader, ISpecIfDataReader dataReader)
		{
			_metadataReader = metadataReader;
            _dataReader = dataReader;
		}

		public Resource CreateResource(Key resourceTypeID)
		{
			Resource result = new Resource();

			ResourceClass resourceType = _metadataReader.GetResourceClassByKey(resourceTypeID);

			result.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
			result.Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID();
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
            if (string.IsNullOrEmpty(data.ID)) // no id given. Create new id and add element as first revision
            {
                data.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
                data.Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID();
                data.Replaces = new List<string>();

            }
            else // id given
            {
                if (data.Revision == null) // no revision set. Set default targetbranch == "main"
                {
                    data.Revision = "";
                    targetBranchName = "main";
                }
                else
                {

                    
                }
            }

            IdentifiableElement parentElement = GetItemWithLatestRevisionInBranch<T>(data.ID, targetBranchName);

            

            // set replacement info
            if (data.Replaces.Count == 0) // no replace given. Take latest of target branch as replacement
            { 
                if (parentElement == null) // branch root element
                {
                    data.Replaces = new List<string>();
                }
                else
                {
                    data.Replaces = new List<string>();
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
        public abstract void MoveNode(string nodeID, string newParentID, int position);
    }
}
