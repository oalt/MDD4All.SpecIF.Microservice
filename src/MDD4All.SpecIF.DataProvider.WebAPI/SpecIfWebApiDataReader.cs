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

        public override List<Hierarchy> GetAllHierarchies()
        {
            Task<List<Hierarchy>> task = GetAllHierarchiesAsync();
            task.Wait();

            return task.Result;
        }

        public async Task<List<Hierarchy>> GetAllHierarchiesAsync()
        {
            List<Hierarchy> result = new List<Hierarchy>();

            string answer = await _httpClient.GetStringAsync(_connectionURL + "/SpecIF/Hierarchy");

            result = JsonConvert.DeserializeObject<List<Hierarchy>>(answer); 

            return result;
        }

        public override byte[] GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public override Hierarchy GetHierarchyByKey(Key key)
        {
            Task<Hierarchy> task = GetDataByKeyAsync<Hierarchy>(key, "/SpecIF/Hierarchy/");
            task.Wait();

            return task.Result;
        }

		public override int GetLatestHierarchyRevision(string hierarchyID)
		{
			Task<int> task = GetLatestRevisionAsync<Hierarchy>(hierarchyID, "SpecIF/Hierarchy/");
			task.Wait();

			return task.Result;
		}

		public override int GetLatestResourceRevision(string resourceID)
		{
			Task<int> task = GetLatestRevisionAsync<Resource>(resourceID, "SpecIF/Resource/");
			task.Wait();

			return task.Result;
		}

		public override int GetLatestStatementRevision(string statementID)
		{
			Task<int> task = GetLatestRevisionAsync<Statement>(statementID, "SpecIF/Statement/");
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

		public async Task<int> GetLatestRevisionAsync<T>(string resourceID, string apiPath)
		{
			int result = 1;

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/LatestRevision/" + resourceID);

			result = int.Parse(answer);

			return result;
		}
	}
}
