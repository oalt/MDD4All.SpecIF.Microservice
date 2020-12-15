/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.WebAPI;
using MDD4All.SpecIF.ServiceDataProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.DataProvider.Integration
{
	public class SpecIfIntegrationDataReader : AbstractSpecIfDataReader
	{

		private SpecIfServiceDataProvider _descriptionProvider;

		private Dictionary<string, ISpecIfDataReader> _dataReaders = new Dictionary<string, ISpecIfDataReader>();

        private bool _cacheActive = false;

        private ISpecIfDataReader _cacheDataReader;
        private ISpecIfDataWriter _cacheDataWriter;


		public SpecIfIntegrationDataReader(SpecIfServiceDataProvider specIfServiceDataProvider)
		{
			_descriptionProvider = specIfServiceDataProvider;
			InitializeReaders();
		}


        public void ActivateCache(ISpecIfDataWriter dataWriter, ISpecIfDataReader dataReader)
        {
            _cacheActive = true;

            _cacheDataReader = dataReader;

            _cacheDataWriter = dataWriter;
        }


		private void InitializeReaders()
		{
			List<SpecIfServiceDescription> serviceDescriptions = _descriptionProvider.GetAvailableServices();

			foreach (SpecIfServiceDescription serviceDescription in serviceDescriptions)
			{
                if (serviceDescription.ID != "{F8B21340-B442-4040-BEFE-CF455BABB3A5}")
                {
                    if (serviceDescription.DataRead == true)
                    {
                        SpecIfWebApiDataReader dataReader = new SpecIfWebApiDataReader(serviceDescription.ServiceAddress + ":" + serviceDescription.ServicePort);

                        dataReader.DataSourceDescription = serviceDescription;

                        _dataReaders.Add(serviceDescription.ID, dataReader);
                    }
                }
			}
		}

		public void AddDataReader(ISpecIfDataReader reader, string id)
		{
			if(!_dataReaders.ContainsKey(id))
			{
				_dataReaders.Add(id, reader);
			}
		}


		public override List<Node> GetAllHierarchies()
		{
			List<Node> result = new List<Node>();

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				List<Node> part = provider.Value.GetAllHierarchies();

				if (part != null && part.Count != 0)
				{
					result.AddRange(part);
				}
			}

			return result;

		}

		public override byte[] GetFile(string filename)
		{
			return new byte[0]; // _metaDataMasterProvider.GetFile(filename);
		}

		public override Node GetHierarchyByKey(Key key)
		{
			Node result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> dataReader in _dataReaders)
			{
				Node hierarchy = dataReader.Value.GetHierarchyByKey(key);

				if (hierarchy != null)
				{
					result = hierarchy;
					break;
				}
			}

			return result;
		}

		//public override Revision GetLatestResourceRevision(string resourceID)
		//{
		//	Revision result = Key.FIRST_MAIN_REVISION;

		//	ISpecIfDataReader provider = FindDataProviderForResource(resourceID);

		//	if (provider != null)
		//	{
		//		result = provider.GetLatestResourceRevision(resourceID);
		//	}

		//	return result;
		//}

		

		public override Resource GetResourceByKey(Key key)
		{
            Resource result = null;

            Resource cachedResource = null;

            if (_cacheActive)
            {
                result = _cacheDataReader.GetResourceByKey(key);
            }
            if (result == null)
            {

                Task<Resource> task = GetResourceByKeyAsync(key);

                task.Wait();

                result = task.Result;

                if (_cacheActive && result != null)
                {
                    _cacheDataWriter.AddResource(result);
                }
            }
            
            return result;
        }

        public async Task<Resource> GetResourceByKeyAsync(Key key)
        {
            Resource result = null;

            List<Task<Resource>> tasks = new List<Task<Resource>>();

            foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
            {
                Task<Resource> resource = ((SpecIfWebApiDataReader)provider.Value).GetResourceByKeyAsync(key);

                

                tasks.Add(resource);
            }

            while (true)
            {
                if (tasks.Count == 0)
                {
                    break;
                }

                Task<Resource> firstFinishedTask = await Task.WhenAny(tasks.ToArray());

                if (firstFinishedTask.Result != null)
                {
                    Console.WriteLine("Task result: " + firstFinishedTask.Result);
                    result = firstFinishedTask.Result;
                    break;
                }
                tasks.Remove(firstFinishedTask);
            }

            return result;

        }
        public override Statement GetStatementByKey(Key key)
		{
			Statement result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				Statement statement = provider.Value.GetStatementByKey(key);

				if (statement != null)
				{
					result = statement;
					break;
				}
			}

			return result;
		}

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			string result = null;

			ISpecIfDataReader provider = FindDataProviderForHierarchy(hierarchyID);

			if (provider != null)
			{
				result = provider.GetLatestHierarchyRevision(hierarchyID);
			}

			return result;
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			string result = null;

			ISpecIfDataReader provider = FindDataProviderForStatement(statementID);

			if (provider != null)
			{
				result = provider.GetLatestStatementRevision(statementID);
			}

			return result;
		}

		private ISpecIfDataReader FindDataProviderForStatement(string statementID)
		{
			ISpecIfDataReader result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				if (provider.Value.GetStatementByKey(new Key() { ID = statementID, Revision = null }) != null)
				{
					result = provider.Value;
					break;
				}
			}

			return result;
		}

		private ISpecIfDataReader FindDataProviderForResource(string id)
		{
			ISpecIfDataReader result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				if (provider.Value.GetResourceByKey(new Key() { ID = id, Revision = null }) != null)
				{
					result = provider.Value;
					break;
				}
			}

			return result;
		}

		private ISpecIfDataReader FindDataProviderForHierarchy(string id)
		{
			ISpecIfDataReader result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				if (provider.Value.GetHierarchyByKey(new Key() { ID = id, Revision = null }) != null)
				{
					result = provider.Value;
					break;
				}
			}

			return result;
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
			List<Statement> result = new List<Statement>();

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				try
				{
					List<Statement> statements = provider.Value.GetAllStatementsForResource(resourceKey);

					if (statements != null)
					{
						result.AddRange(statements);
					}
				}
				catch(Exception exception)
                {

                }
			}

			return result;
		}

		public override List<Node> GetContainingHierarchyRoots(Key resourceKey)
		{
			throw new System.NotImplementedException();
		}



        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            throw new System.NotImplementedException();
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            throw new System.NotImplementedException();
        }

        public override List<Statement> GetAllStatements()
        {
            throw new System.NotImplementedException();
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
        {
            List<Node> result = new List<Node>();

            foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
            {
                List<Node> childNodes = provider.Value.GetChildNodes(parentNodeKey);

                if (childNodes != null)
                {
                    result = childNodes;
                    break;
                }
            }

            return result;
        }

        public override string GetLatestResourceRevisionForBranch(string resourceID, string branchName)
        {
            throw new System.NotImplementedException();
        }

        public override Node GetNodeByKey(Key key)
        {
            Node result = null;

            foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
            {
                Node node = provider.Value.GetNodeByKey(key);

                if (node != null)
                {
                    result = node;
                    break;
                }
            }

            return result;
        }

        public override Node GetParentNode(Key childNode)
        {
            throw new System.NotImplementedException();
        }


        public override List<ProjectDescriptor> GetProjectDescriptions()
        {
            List<ProjectDescriptor> result = new List<ProjectDescriptor>();

            foreach (KeyValuePair<string, ISpecIfDataReader> dataReader in _dataReaders)
            {
                List<ProjectDescriptor> projectDescriptions = dataReader.Value.GetProjectDescriptions();

                if (projectDescriptions != null && projectDescriptions.Count > 0)
                {
                    result.AddRange(projectDescriptions);
                }
            }

            return result;
        }

        public override List<Node> GetAllHierarchyRootNodes(string projectID = null)
        {
            List<Node> result = new List<Node>();

            foreach (KeyValuePair<string, ISpecIfDataReader> dataReader in _dataReaders)
            {
                List<Node> hierarchies = dataReader.Value.GetAllHierarchyRootNodes(projectID);

                if (hierarchies != null && hierarchies.Count > 0)
                {
                    result.AddRange(hierarchies);
                }
            }

            return result;
        }

        public override DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, string projectID, List<Key> hierarchyFilter = null, bool includeMetadata = true)
        {
            throw new System.NotImplementedException();
        }
    }
}
