/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MDD4All.SpecIF.DataProvider.WebAPI
{
    public class SpecIfWebApiDataWriter : AbstractSpecIfDataWriter
    {
        private string _connectionURL;

        private HttpClient _httpClient = new HttpClient();

        public SpecIfWebApiDataWriter(string webApiConnectionURL,
                                      LoginData loginData,
                                      ISpecIfMetadataReader metadataReader, 
                                      ISpecIfDataReader dataReader) : base(metadataReader, dataReader)
        {
            _connectionURL = webApiConnectionURL;

            _httpClient.DefaultRequestHeaders.Accept.Clear();

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            SetAuthorizationHeaderForApi(loginData);

        }

        private void SetAuthorizationHeaderForApi(LoginData loginData)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/oauth/token");

            Task<JwtAccessToken> tokenRequestTask = PostDataAsync<LoginData, JwtAccessToken>(uriBuilder.Uri, loginData);

            tokenRequestTask.Wait();

            JwtAccessToken jwtAccessToken = tokenRequestTask.Result;

            if(jwtAccessToken != null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtAccessToken.AccessToken);
            }

        }

        public override void AddHierarchy(Node hierarchy, string projectID = null)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            if (projectID != null)
            {
                parameters["projectID"] = projectID;

                uriBuilder.Query = parameters.ToString();
            }

            PostDataAsync<Node, Node>(uriBuilder.Uri, hierarchy).Wait();
        }

        public override void AddNodeAsFirstChild(string parentNodeID, Node newNode)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            if (parentNodeID != null)
            {
                parameters["parentNodeId"] = parentNodeID;

                uriBuilder.Query = parameters.ToString();
            }

            

            PostDataAsync<Node, Node>(uriBuilder.Uri, newNode).Wait();

        }

        public override void AddProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project, string integrationID = null)
        {
            throw new NotImplementedException();
        }

        public override void AddResource(Resource resource)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/resources");
            PostDataAsync<Resource, Resource>(uriBuilder.Uri, resource).Wait();
        }

        public override void AddStatement(Statement statement)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/statements/");

            PostDataAsync<Statement, Statement>(uriBuilder.Uri, statement).Wait();
        }

        public override void DeleteProject(string projectID)
        {
            throw new NotImplementedException();
        }

        public override void InitializeIdentificators()
        {
            //throw new NotImplementedException();
        }

        public override void MoveNode(string nodeID, string newParentID, string newSiblingId)
        {
            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/move");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            if (nodeID != null)
            {
                parameters["nodeId"] = nodeID;
            }

            if (newParentID != null)
            {
                parameters["newParentId"] = newParentID;
            }

            if (newSiblingId != null)
            {
                parameters["newSiblingId"] = newSiblingId;
            }

            uriBuilder.Query = parameters.ToString();

            Task<HttpResponseMessage> responseMessageTask = PutCommandAsync(uriBuilder.Uri);

            responseMessageTask.Wait();

            HttpResponseMessage responseMessage = responseMessageTask.Result;
        }



        public override Node UpdateHierarchy(Node hierarchyToUpdate, string parentID = null, string predecessorID = null)
        {
            Node result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/hierarchies/");

            Uri finalUrl = uriBuilder.Uri;


            Task<Node> resourceTask = PutDataAsync<Node, Node>(finalUrl, hierarchyToUpdate);

            resourceTask.Wait();

            result = resourceTask.Result;

            return result;
        }

        public override void SaveIdentificators()
        {
            //throw new NotImplementedException();
        }

        public override Resource SaveResource(Resource resource, string projectID = null)
        {
            Resource result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/resources/");

            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(string.Empty);

            if (projectID != null)
            {
                parameters["projectID"] = projectID;
            }

            uriBuilder.Query = parameters.ToString();

            Uri finalUrl = uriBuilder.Uri;
            

            Task<Resource> resourceTask = PostDataAsync<Resource, Resource>(finalUrl, resource);

            resourceTask.Wait();

            result = resourceTask.Result;

            return result;
        }

        public override Statement SaveStatement(Statement statement, string projectID = null)
        {
            throw new NotImplementedException();
        }



        public override void UpdateProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project)
        {
            throw new NotImplementedException();
        }

        public override Resource UpdateResource(Resource resource)
        {
            Resource result = null;

            UriBuilder uriBuilder = new UriBuilder(_connectionURL + "/specif/v1.0/resources/");

            Uri finalUrl = uriBuilder.Uri;

            Task<Resource> resourceTask = PutDataAsync<Resource, Resource>(finalUrl, resource);

            resourceTask.Wait();

            result = resourceTask.Result;

            return result;
        }

        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
        {
            throw new NotImplementedException();
        }

        private async Task<TResult> PostDataAsync<T, TResult>(Uri url, T data)
        {
            TResult result = default(TResult);

            try
            {
                string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync(url, stringContent);

                string jiraResult = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<TResult>(jiraResult);
            }
            catch(Exception exception)
            {
                Debug.WriteLine(exception);
            }

            return result;
        }

        private async Task<TResult> PutDataAsync<T, TResult>(Uri url, T data)
        {
            TResult result = default(TResult);

            try
            {
                string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                StringContent stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PutAsync(url, stringContent);

                string jiraResult = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<TResult>(jiraResult);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }

            return result;
        }

        private async Task<HttpResponseMessage> PutCommandAsync(Uri url)
        {
            HttpResponseMessage result = null;

            try
            {
                
                StringContent stringContent = new StringContent("", Encoding.UTF8, "application/json");
                result = await _httpClient.PutAsync(url, stringContent);

                
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }

            return result;
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
