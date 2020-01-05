/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataProvider.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.DataProvider.WebAPI
{
	public class SpecIfWebApiDataWriter : AbstractSpecIfDataWriter
	{
		private string _connectionURL;

		private HttpClient _httpClient = new HttpClient();

		public SpecIfWebApiDataWriter(string webApiConnectionURL, ISpecIfMetadataReader metadataReader) : base(metadataReader)
		{
			_connectionURL = webApiConnectionURL;
		}

		public override void AddHierarchy(Node hierarchy)
		{
			PostDataAsync(_connectionURL + "/Hierarchy", hierarchy).Wait();
		}

		//public override void AddNode(Node newNode)
		//{
		//	PostDataAsync(_connectionURL + "/Node", newNode).Wait();
		//}

        public override void AddNode(string parentNodeID, Node newNode)
        {
            throw new NotImplementedException();
        }

        public override void AddResource(Resource resource)
		{
			PostDataAsync(_connectionURL + "/Resource", resource).Wait();
		}

		public override void AddStatement(Statement statement)
		{
			PostDataAsync(_connectionURL + "/Statement", statement).Wait();
		}

		public override void InitializeIdentificators()
		{
			//throw new NotImplementedException();
		}

        public override Node SaveHierarchy(Node hierarchyToUpdate)
        {
            throw new NotImplementedException();
        }

        public override void SaveIdentificators()
		{
			//throw new NotImplementedException();
		}

        public override Node SaveNode(Node nodeToUpdate)
        {
            throw new NotImplementedException();
        }

        public override Resource SaveResource(Resource resource)
        {
            throw new NotImplementedException();
        }

        public override Statement SaveStatement(Statement statement)
        {
            throw new NotImplementedException();
        }

  //      public override void UpdateHierarchy(Node hierarchyToUpdate)
		//{
		//	PutDataAsync(_connectionURL + "/Hierarchy", hierarchyToUpdate).Wait();
		//}

		//public override void UpdateNode(Node nodeToUpdate)
		//{
		//	PutDataAsync(_connectionURL + "/Node", nodeToUpdate).Wait();
		//}

		//public override void UpdateResource(Resource resource)
		//{
		//	PutDataAsync(_connectionURL + "/Resource", resource).Wait();
		//}

		//public override void UpdateStatement(Statement statement)
		//{
		//	throw new NotImplementedException();
		//}

        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
        {
            throw new NotImplementedException();
        }

        private async Task<HttpResponseMessage> PostDataAsync<T>(string url, T data)
		{
			StringContent stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			return await _httpClient.PostAsync(url, stringContent);
		}

		private async Task<HttpResponseMessage> PutDataAsync<T>(string url, T data)
		{
			StringContent stringContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			return await _httpClient.PutAsync(url, stringContent);
		}
	}
}
