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
using System.Text;

namespace MDD4All.SpecIF.DataProvider.Integration
{
	public class SpecIfIntegrationDataWriter : AbstractSpecIfDataWriter
	{
		private SpecIfServiceDataProvider _descriptionProvider;

		private Dictionary<string, ISpecIfDataWriter> _dataWriters = new Dictionary<string, ISpecIfDataWriter>();

		// just for demo
		private List<ISpecIfDataWriter> _demoDataWriterList = new List<ISpecIfDataWriter>();

		public SpecIfIntegrationDataWriter(SpecIfServiceDataProvider specIfServiceDataProvider, SpecIfIntegrationMetadataReader metadataReader) : base(metadataReader)
		{
			_descriptionProvider = specIfServiceDataProvider;
			InitializeWriters();
		}

		private void InitializeWriters()
		{
			List<SpecIfServiceDescription> serviceDescriptions = _descriptionProvider.GetAvailableServices();

			foreach (SpecIfServiceDescription serviceDescription in serviceDescriptions)
			{
				if (serviceDescription.DataRead == true)
				{
					SpecIfWebApiDataWriter dataWriter = new SpecIfWebApiDataWriter(serviceDescription.ServiceAddress + ":" + serviceDescription.ServicePort, _metadataReader);

					dataWriter.DataSourceDescription = serviceDescription;

					_dataWriters.Add(serviceDescription.ID, dataWriter);
					_demoDataWriterList.Add(dataWriter);
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


		public override void AddHierarchy(Node hierarchy)
		{
			FindDataWriterForHierarchy(hierarchy).AddHierarchy(hierarchy);
		}

		public override void AddNode(Node newNode)
		{
			FindDataProviderForNode(newNode).AddNode(newNode);
		}

		public override void AddResource(Resource resource)
		{
			FindDataProviderForResource(resource).AddResource(resource);
		}

		public override void AddStatement(Statement statement)
		{
			FindDataProviderForStatement(statement).AddStatement(statement);
		}

		public override void UpdateHierarchy(Node hierarchyToUpdate)
		{
			ISpecIfDataWriter writer = FindDataWriterForHierarchy(hierarchyToUpdate);

			if (writer != null)
			{
				writer.UpdateHierarchy(hierarchyToUpdate);
			}
		}

		public override void UpdateNode(Node nodeToUpdate)
		{
			ISpecIfDataWriter provider = FindDataProviderForNode(nodeToUpdate);

			if (provider != null)
			{
				provider.UpdateNode(nodeToUpdate);
			}
		}

		public override void UpdateResource(Resource resource)
		{
			ISpecIfDataWriter provider = FindDataProviderForResource(resource);

			if (provider != null)
			{
				provider.UpdateResource(resource);
			}
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
			return _demoDataWriterList[0];

			ISpecIfDataWriter result = _dataWriters[resource.DataSource.ID];

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
	}
}
