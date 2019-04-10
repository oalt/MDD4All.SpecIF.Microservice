/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MDD4All.SpecIF.DataProvider.WebAPI
{
    public class SpecIfWebApiDataReader : AbstractSpecIfDataReader
    {
        private string _connectionURL;

        private HttpClient _httpClient = new HttpClient();

        public SpecIfWebApiDataReader(string webApiConnectionURL)
        {
            _connectionURL = webApiConnectionURL;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public override List<Node> GetAllHierarchies()
        {
            Task<List<Node>> task = GetAllHierarchiesAsync();
            task.Wait();

            return task.Result;
        }

        public async Task<List<Node>> GetAllHierarchiesAsync()
        {
            List<Node> result = new List<Node>();

            string answer = await _httpClient.GetStringAsync(_connectionURL + "/SpecIF/Hierarchy");

            result = JsonConvert.DeserializeObject<List<Node>>(answer); 

            return result;
        }

        public override byte[] GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public override Node GetHierarchyByKey(Key key)
        {
            Task<Node> task = GetDataByKeyAsync<Node>(key, "/SpecIF/Hierarchy/");
            task.Wait();

            return task.Result;
        }

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			Task<string> task = GetLatestRevisionAsync<Node>(hierarchyID, "SpecIF/Hierarchy/");
			task.Wait();

			return task.Result;
		}

		public override string GetLatestResourceRevision(string resourceID)
		{
			Task<string> task = GetLatestRevisionAsync<Resource>(resourceID, "SpecIF/Resource/");
			task.Wait();

			return task.Result;
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			Task<string> task = GetLatestRevisionAsync<Statement>(statementID, "SpecIF/Statement/");
			task.Wait();

			return task.Result;
		}

		public override Resource GetResourceByKey(Key key)
        {
			Resource result;

            Task<Resource> task = GetDataByKeyAsync<Resource>(key, "/SpecIF/Resource/");
            task.Wait();

			result = task.Result;

			if (result != null)
			{
				result.DataSource = DataSourceDescription;
			}
            return task.Result;
        }

		

		public override Statement GetStatementByKey(Key key)
		{
			Task<Statement> task = GetDataByKeyAsync<Statement>(key, "/SpecIF/Statement/");
			task.Wait();

			return task.Result;
		}

		private async Task<T> GetDataByKeyAsync<T>(Key key, string apiPath)
		{
			T result = default(T);

			try
			{
				string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/" + key.ID);

				result = JsonConvert.DeserializeObject<T>(answer);
			}
			catch (Exception exception)
			{
				Console.WriteLine("[ERROR] key=" + key.ID + "--" + key.Revision);
				Console.WriteLine(exception);
			}

			return result;
		}

		public async Task<string> GetLatestRevisionAsync<T>(string resourceID, string apiPath)
		{
			string result = Key.FIRST_MAIN_REVISION;

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/LatestRevision/" + resourceID);

			result = answer;

			return result;
		}
	}
}
