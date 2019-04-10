/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Service;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.WebAPI;
using MDD4All.SpecIF.ServiceDataProvider;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Integration
{
	public class SpecIfIntegrationDataReader : AbstractSpecIfDataReader
	{

		private SpecIfServiceDataProvider _descriptionProvider;

		private Dictionary<string, ISpecIfDataReader> _dataReaders = new Dictionary<string, ISpecIfDataReader>();


		public SpecIfIntegrationDataReader(SpecIfServiceDataProvider specIfServiceDataProvider)
		{
			_descriptionProvider = specIfServiceDataProvider;
			InitializeReaders();
		}


		private void InitializeReaders()
		{
			List<SpecIfServiceDescription> serviceDescriptions = _descriptionProvider.GetAvailableServices();

			foreach (SpecIfServiceDescription serviceDescription in serviceDescriptions)
			{
				if (serviceDescription.DataRead == true)
				{
					SpecIfWebApiDataReader dataReader = new SpecIfWebApiDataReader(serviceDescription.ServiceAddress + ":" + serviceDescription.ServicePort);

					dataReader.DataSourceDescription = serviceDescription;

					_dataReaders.Add(serviceDescription.ID, dataReader);
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

		public override string GetLatestResourceRevision(string resourceID)
		{
			string result = Key.FIRST_MAIN_REVISION;

			ISpecIfDataReader provider = FindDataProviderForResource(resourceID);

			if (provider != null)
			{
				result = provider.GetLatestResourceRevision(resourceID);
			}

			return result;
		}

		

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			foreach (KeyValuePair<string, ISpecIfDataReader> provider in _dataReaders)
			{
				Resource resource = provider.Value.GetResourceByKey(key);

				if (resource != null)
				{
					result = resource;
					break;
				}
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
			string result = Key.FIRST_MAIN_REVISION;

			ISpecIfDataReader provider = FindDataProviderForHierarchy(hierarchyID);

			if (provider != null)
			{
				result = provider.GetLatestHierarchyRevision(hierarchyID);
			}

			return result;
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			string result = Key.FIRST_MAIN_REVISION;

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
				if (provider.Value.GetStatementByKey(new Key() { ID = statementID, Revision = Key.LATEST_REVISION }) != null)
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
				if (provider.Value.GetResourceByKey(new Key() { ID = id, Revision = Key.LATEST_REVISION }) != null)
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
				if (provider.Value.GetHierarchyByKey(new Key() { ID = id, Revision = Key.LATEST_REVISION }) != null)
				{
					result = provider.Value;
					break;
				}
			}

			return result;
		}
	}
}
