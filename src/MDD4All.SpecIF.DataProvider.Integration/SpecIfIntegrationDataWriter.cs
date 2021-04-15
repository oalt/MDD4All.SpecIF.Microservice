/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.WebAPI;
using MDD4All.SpecIF.ServiceDataProvider;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Integration
{
    public class SpecIfIntegrationDataWriter : AbstractSpecIfDataWriter
	{
		private SpecIfServiceDataProvider _descriptionProvider;

        private Dictionary<string, ISpecIfDataWriter> _dataWriters = new Dictionary<string, ISpecIfDataWriter>();

		// just for demo
		private List<ISpecIfDataWriter> _demoDataWriterList = new List<ISpecIfDataWriter>();

		public SpecIfIntegrationDataWriter(string integrationApiKey,
                                           SpecIfServiceDataProvider specIfServiceDataProvider, 
                                           SpecIfIntegrationMetadataReader metadataReader,
                                           SpecIfIntegrationDataReader dataReader) : base(metadataReader, dataReader)
		{
			_descriptionProvider = specIfServiceDataProvider;
			InitializeWriters(integrationApiKey);
		}

		private void InitializeWriters(string integrationApiKey)
		{
			List<SpecIfServiceDescription> serviceDescriptions = _descriptionProvider.GetAvailableServices();

            foreach (SpecIfServiceDescription serviceDescription in serviceDescriptions)
            {
                if (serviceDescription.ID != "{F8B21340-B442-4040-BEFE-CF455BABB3A5}") // exclude integration service 
                {
                    if (serviceDescription.DataWrite == true)
                    {
                        
                        SpecIfWebApiDataWriter dataWriter = new SpecIfWebApiDataWriter(serviceDescription.ServiceAddress + ":" + serviceDescription.ServicePort,
                                                                                       integrationApiKey,
                                                                                       _metadataReader,
                                                                                       _dataReader);

                        dataWriter.DataSourceDescription = serviceDescription;

                        _dataWriters.Add(serviceDescription.ID, dataWriter);
                        _demoDataWriterList.Add(dataWriter);
                    }
                }
            }
		}

        

		public void AddDataWriter(ISpecIfDataWriter dataWriter, string id)
		{
			if (!_dataWriters.ContainsKey(id))
			{
				_dataWriters.Add(id, dataWriter);
			}
		}

		public override void AddResource(Resource resource)
		{
			FindDataProviderForResource(resource).AddResource(resource);
		}

		public override void AddStatement(Statement statement)
		{
			FindDataProviderForStatement(statement).AddStatement(statement);
		}

		public override void InitializeIdentificators()
		{
			//_metaDataMasterProvider.InitializeIdentificators();
		}

		public override void SaveIdentificators()
		{
			//_metaDataMasterProvider.SaveIdentificators();
		}


		private ISpecIfDataWriter FindDataWriterForHierarchy(Node hierarchy)
		{
			return _demoDataWriterList[0];

			//ISpecIfDataWriter result = _dataWriters[hierarchy.DataSource.ID];

			//return result;


		}

		private ISpecIfDataWriter FindDataProviderForResource(Resource resource)
		{
            ISpecIfDataWriter result = null;

            //TODO Replace hard coded types
            if (resource.Class.ID == "RC-Requirement")
            {
                ISpecIfDataWriter jiraDataWriter = _dataWriters["{DFFE5123-E1D0-4E24-B37C-8CF019BEB7EE}"];

                if (jiraDataWriter != null)
                {
                    result = jiraDataWriter;
                }
            }

            if (result == null)
            {
                ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

                if (mongodbDataWriter != null)
                {
                    result = mongodbDataWriter;
                }
            }

            return result;
		}

		private ISpecIfDataWriter FindDataProviderForStatement(Statement statement)
		{
			return _demoDataWriterList[0];

			ISpecIfDataWriter result = _dataWriters[statement.DataSource.ID];

			return result;
		}

		private ISpecIfDataWriter FindDataProviderForNode(Node node)
		{
			return _demoDataWriterList[0];

			ISpecIfDataWriter result = _dataWriters[node.DataSource.ID];

			return result;
		}

		//public override void UpdateStatement(Statement statement)
		//{
		//	FindDataProviderForStatement(statement).UpdateStatement(statement);
		//}

        public override Node UpdateHierarchy(Node hierarchyToUpdate, string parentID = null, string predecessorID = null)
        {
            Node result = null;

            ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

            if (mongodbDataWriter != null)
            {
                result = mongodbDataWriter.UpdateHierarchy(hierarchyToUpdate);
            }

            return result;
        }

        public override Resource SaveResource(Resource resource, string projectID = null)
        {
            Resource result = null;

            ISpecIfDataWriter specIfDataWriter = FindDataProviderForResource(resource);

            if(specIfDataWriter != null)
            {
                result = specIfDataWriter.SaveResource(resource, projectID);
            }

            return result;
        }


        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
        {
            throw new NotImplementedException();
        }

        public override void AddNodeAsFirstChild(string parentNodeID, Node newNode)
        {
            ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

            if (mongodbDataWriter != null)
            {
                mongodbDataWriter.AddNodeAsFirstChild(parentNodeID, newNode);
            }
        }

        public override void AddHierarchy(Node hierarchy, string projectID = null)
        {
            ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

            if (mongodbDataWriter != null)
            {
                mongodbDataWriter.AddHierarchy(hierarchy, projectID);
            }
        }

        public override void MoveNode(string nodeID, string newParentID, string newSiblingId)
        {
            ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

            if (mongodbDataWriter != null)
            {
                mongodbDataWriter.MoveNode(nodeID, newParentID, newSiblingId);
            }
        }

        public override Resource UpdateResource(Resource resource)
        {
            Resource result = null;

            //TODO Replace hard coded types
            if (resource.Class.ID == "RC-Requirement")
            {
                ISpecIfDataWriter jiraDataWriter = _dataWriters["{DFFE5123-E1D0-4E24-B37C-8CF019BEB7EE}"];

                if (jiraDataWriter != null)
                {
                    result = jiraDataWriter.UpdateResource(resource);
                }
            }

            if (result == null)
            {
                ISpecIfDataWriter mongodbDataWriter = _dataWriters["{67FE892C-7EB1-45AD-9259-6BE910841A3A}"];

                if (mongodbDataWriter != null)
                {
                    result = mongodbDataWriter.UpdateResource(resource);
                }
            }

            return result;
        }

        public override void AddProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project, string integrationID = null)
        {
            throw new NotImplementedException();
        }



        public override void DeleteProject(string projectID)
        {
            throw new NotImplementedException();
        }

        public override Statement SaveStatement(Statement statement, string projectID = null)
        {
            throw new NotImplementedException();
        }

        

        public override void UpdateProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project)
        {
            throw new NotImplementedException();
        }

        public override void AddNodeAsPredecessor(string predecessorID, Node newNode)
        {
            throw new NotImplementedException();
        }

        public override void DeleteNode(string nodeID)
        {
            throw new NotImplementedException();
        }
    }
}
