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
	public class SpecIfWebApiMetadataReader : AbstractSpecIfMetadataReader
	{

		private string _connectionURL;

		private HttpClient _httpClient = new HttpClient();

		public SpecIfWebApiMetadataReader(string webApiConnectionURL)
		{
			_connectionURL = webApiConnectionURL;
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public override List<DataType> GetAllDataTypes()
		{
			Task<List<DataType>> task = GetDataListAsync<DataType>("/SpecIF/DataType");
			task.Wait();

			return task.Result;
		}


		public override List<PropertyClass> GetAllPropertyClasses()
		{
			Task<List<PropertyClass>> task = GetDataListAsync<PropertyClass>("/SpecIF/PropertyClass");
			task.Wait();

			return task.Result;
		}

		public override List<ResourceClass> GetAllResourceClasses()
		{
			Task<List<ResourceClass>> task = GetDataListAsync<ResourceClass>("/SpecIF/ResourceClass");
			task.Wait();

			return task.Result;
		}

		


		public override ResourceClass GetResourceClassByKey(Key key)
		{
			Task<ResourceClass> task = GetDataByKeyAsync<ResourceClass>(key, "/SpecIF/ResourceClass");
			task.Wait();

			return task.Result;
		}

		public override StatementClass GetStatementClassByKey(Key key)
		{
			Task<StatementClass> task = GetDataByKeyAsync<StatementClass>(key, "/SpecIF/StatementClass");
			task.Wait();

			return task.Result;
		}

		public override PropertyClass GetPropertyClassByKey(Key key)
		{
			Task<PropertyClass> task = GetDataByKeyAsync<PropertyClass>(key, "/SpecIF/PropertyClass");
			task.Wait();

			return task.Result;
		}

		public override string GetLatestPropertyClassRevision(string propertyClassID)
		{
			Task<string> task = GetLatestRevisionAsync<string>(propertyClassID, "SpecIF/PropertyClass/");
			task.Wait();

			return task.Result;
		}

		public override string GetLatestResourceClassRevision(string resourceClassID)
		{
			Task<string> task = GetLatestRevisionAsync<string>(resourceClassID, "SpecIF/ResourceClass/");
			task.Wait();

			return task.Result;
		}

		public override string GetLatestStatementClassRevision(string statementClassID)
		{
			Task<string> task = GetLatestRevisionAsync<string>(statementClassID, "SpecIF/StatementClass/");
			task.Wait();

			return task.Result;
		}

		private async Task<List<T>> GetDataListAsync<T>(string apiPath)
		{
			List<T> result = new List<T>();

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath);

			result = JsonConvert.DeserializeObject<List<T>>(answer);

			return result;
		}

		private async Task<T> GetDataById<T>(string id, string apiPath)
		{
			T result = default(T);

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/" + id);

			result = JsonConvert.DeserializeObject<T>(answer);

			return result;
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
			string result = "";

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/LatestRevision/" + resourceID);

			//result = new Revision(answer);

			return result;
		}

        public override DataType GetDataTypeByKey(Key key)
        {

            Task<DataType> task = GetDataById<DataType>(key.ID, "/SpecIF/DataType");
            task.Wait();

            return task.Result;
        }

        public override List<StatementClass> GetAllStatementClasses()
        {
            throw new NotImplementedException();
        }

        public override List<DataType> GetAllDataTypeRevisions(string dataTypeID)
        {
            throw new NotImplementedException();
        }

        public override List<PropertyClass> GetAllPropertyClassRevisions(string propertyClassID)
        {
            throw new NotImplementedException();
        }

        public override List<ResourceClass> GetAllResourceClassRevisions(string resourceClassID)
        {
            throw new NotImplementedException();
        }

        public override List<StatementClass> GetAllStatementClassRevisions(string statementClassID)
        {
            throw new NotImplementedException();
        }
    }
}
