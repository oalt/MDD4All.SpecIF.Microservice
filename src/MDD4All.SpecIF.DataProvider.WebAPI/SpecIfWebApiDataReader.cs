/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace MDD4All.SpecIF.DataProvider.WebAPI
{
    public class SpecIfWebApiDataReader : AbstractSpecIfDataReader
    {
        private string _connectionURL;

        private HttpClient _httpClient = new HttpClient();

        public SpecIfWebApiDataReader(string webApiConnectionURL, string authorizationHeaderValue = null)
        {
            _connectionURL = webApiConnectionURL;

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (authorizationHeaderValue != null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", authorizationHeaderValue);
            }
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

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies");

            Uri finalUrl = uriBuilder.Uri;

            string answer = await _httpClient.GetStringAsync(finalUrl);

            result = JsonConvert.DeserializeObject<List<Node>>(answer); 

            return result;
        }

        public override byte[] GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public override Node GetHierarchyByKey(Key key)
        {
            Node result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/" + key.ID);

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["revision"] = key.Revision;

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Task<Node> task = GetDataFromServiceAsync<Node>(finalUrl);
            task.Wait();

            result = task.Result;

            return result;
        }

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			Task<string> task = GetLatestRevisionAsync<Node>(hierarchyID, "SpecIF/Hierarchy/");
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

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/resources/" + key.ID);

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["revision"] = key.Revision;
           
            uriBuilder.Query = parameters.ToString();
           
            Uri finalUrl = uriBuilder.Uri;

            Debug.WriteLine(finalUrl);

            Task<Resource> task = GetDataFromServiceAsync<Resource>(finalUrl);
            task.Wait();

			result = task.Result;

			if (result != null)
			{
				result.DataSource = DataSourceDescription;
			}

            return result;
        }

		public async Task<Resource> GetResourceByKeyAsync(Key key)
        {
            Resource result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/resources/" + key.ID);

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["revision"] = key.Revision;

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Debug.WriteLine(finalUrl);

            result = await GetDataFromServiceAsync<Resource>(finalUrl);

            return result;
        }

		public override Statement GetStatementByKey(Key key)
		{
            Statement result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/statements/" + key.ID);

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["revision"] = key.Revision;

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Task<Statement> task = GetDataFromServiceAsync<Statement>(finalUrl);
			task.Wait();

            result = task.Result;

            return result;
		}

		private async Task<T> GetDataFromServiceAsync<T>( Uri uri)
		{
			T result = default(T);

			try
			{
				string answer = await _httpClient.GetStringAsync(uri);

				result = JsonConvert.DeserializeObject<T>(answer, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			}
			catch (Exception exception)
			{
				
				//Console.WriteLine(exception);
			}

			return result;
		}

		public async Task<string> GetLatestRevisionAsync<T>(string resourceID, string apiPath)
		{
			string result = null;

			string answer = await _httpClient.GetStringAsync(_connectionURL + apiPath + "/LatestRevision/" + resourceID);



            

			return result;
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
            List<Statement> result = new List<Statement>();

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/statements");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["subjectID"] = resourceKey.ID;
            parameters["subjectRevision"] = resourceKey.Revision;
            parameters["objectID"] = resourceKey.ID;
            parameters["objectRevision"] = resourceKey.Revision;

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Task<List<Statement>> task = GetDataFromServiceAsync<List<Statement>>(finalUrl);
            task.Wait();

            result = task.Result;

            return result;
        }

		public override List<Node> GetContainingHierarchyRoots(Key resourceKey)
		{
			throw new NotImplementedException();
		}

        

        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatements()
        {
            throw new NotImplementedException();
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
        {
            throw new NotImplementedException();
        }

        public override string GetLatestResourceRevisionForBranch(string resourceID, string branchName)
        {
            throw new NotImplementedException();
        }

        public override Node GetNodeByKey(Key key)
        {
            throw new NotImplementedException();
        }

        public override Node GetParentNode(Key childNode)
        {
            throw new NotImplementedException();
        }


        public override List<ProjectDescriptor> GetProjectDescriptions()
        {
            List<ProjectDescriptor> result;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/projects");
            
            Task<List<ProjectDescriptor>> task = GetDataFromServiceAsync<List<ProjectDescriptor>>(uriBuilder.Uri);
            task.Wait();

            result = task.Result;

            return task.Result;
        }

        public override List<Node> GetAllHierarchyRootNodes(string projectID = null)
        {
            List<Node> result;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["rootNodesOnly"] = true.ToString();

            if (projectID != null)
            {
                parameters["project"] = projectID;
            }

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Debug.WriteLine(finalUrl);

            Task<List<Node>> task = GetDataFromServiceAsync<List<Node>>(finalUrl);
            task.Wait();

            result = task.Result;

            
            return task.Result;
        }

        public override DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, string projectID, List<Key> hierarchyFilter = null, bool includeMetadata = true)
        {
            DataModels.SpecIF result;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/projects/" + projectID);

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["includeMetedata"] = includeMetadata.ToString();

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;

            Debug.WriteLine(finalUrl);

            Task<DataModels.SpecIF> task = GetDataFromServiceAsync<DataModels.SpecIF>(finalUrl);
            task.Wait();

            result = task.Result;


            return task.Result;
        }
    }
}
