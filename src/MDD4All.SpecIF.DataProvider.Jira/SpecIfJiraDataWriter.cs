using MDD4All.Jira.DataModels;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataProvider.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataAccess.Jira;

namespace MDD4All.SpecIF.DataProvider.Jira
{
    public class SpecIfJiraDataWriter : AbstractSpecIfDataWriter
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private string _url;

        private string _apiKey;

        public SpecIfJiraDataWriter(string url, 
                                    string apiKey, 
                                    ISpecIfMetadataReader metadataReader, 
                                    ISpecIfDataReader dataReader) : base(metadataReader, dataReader)
        {
            _url = url;
            _apiKey = apiKey;

            _httpClient.DefaultRequestHeaders.Accept.Clear();

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.DefaultRequestHeaders.Add("Authorization", _apiKey);

        }

        public override void AddHierarchy(Node hierarchy, string projectID = null)
        {
            throw new NotImplementedException();
        }

        public override void AddNodeAsFirstChild(string parentNodeID, Node newNode)
        {
            //throw new NotImplementedException();
        }

        public override void AddProject(ISpecIfMetadataWriter metadataWriter, DataModels.SpecIF project, string integrationID = null)
        {
            throw new NotImplementedException();
        }

        public override void AddResource(Resource resource)
        {

            throw new NotImplementedException();

        }

        public override Resource SaveResource(Resource resource, string projectID = null)
        {
            Resource result = null;

            //projectID = "_d383255183995289153c412de3b5bffda9bcf45b_TEMPLATE";

            if (projectID != null)
            {
                if (_dataReader is SpecIfJiraDataReader)
                {
                    SpecIfJiraDataReader dataReader = _dataReader as SpecIfJiraDataReader;

                    Project jiraProject = dataReader.GetJiraProjectInfo(projectID);

                    string jiraTypeID = GetJiraTypeFromSpecIfClass(resource.Class, projectID);

                    if(jiraProject != null && jiraTypeID != null)
                    {
                        string title = resource.GetPropertyValue("dcterms:title", _metadataReader);

                        string description = resource.GetPropertyValue("dcterms:description", _metadataReader);

                        SpecIfToJiraConverter specIfToJiraConverter = new SpecIfToJiraConverter();

                        Issue newIssue = new Issue
                        {
                            Fields = new Fields
                            {
                                Project = new Project
                                {
                                    ID = jiraProject.ID
                                },
                                Summary = title,
                                Description = specIfToJiraConverter.ConvertDescription(description),
                                IssueType = new IssueType
                                {
                                    ID = jiraTypeID
                                }
                            }
                        };

                        Task<Issue> issueCreateTask = CreateJiraIssueAsync(newIssue);

                        issueCreateTask.Wait();

                        Issue newJiraResultIssue = issueCreateTask.Result;

                        if(newJiraResultIssue != null)
                        {
                            string newIssueGuid = JiraGuidConverter.ConvertToSpecIfGuid(newJiraResultIssue.Self, newJiraResultIssue.ID);

                            result = _dataReader.GetResourceByKey(new Key(newIssueGuid));

                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classKey"></param>
        /// <param name="projectID">SpecIF project ID.</param>
        /// <returns></returns>
        private string GetJiraTypeFromSpecIfClass(Key classKey, string projectID)
        {
            string result = null;

            if(_dataReader is SpecIfJiraDataReader)
            {
                SpecIfJiraDataReader dataReader = _dataReader as SpecIfJiraDataReader;

                Project jiraProject = dataReader.GetJiraProjectInfo(projectID);

                if (jiraProject != null)
                {
                    if (classKey.ID == "RC-Requirement" && classKey.Revision == "1")
                    {
                        IssueType requirementIssueType = jiraProject.IssueTypes.Find(issueType => issueType.Name == "Requirement");

                        if (requirementIssueType != null)
                        {
                            result = requirementIssueType.ID;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<Issue> CreateJiraIssueAsync(Issue issue)
        {
            Issue result = null;

            try
            {
                string json = JsonConvert.SerializeObject(issue, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(_url + "/rest/api/2/issue", data);

                string jiraResult = response.Content.ReadAsStringAsync().Result;

                result = JsonConvert.DeserializeObject<Issue>(jiraResult);
            }
            catch(Exception exception)
            {
                Debug.WriteLine(exception);
            }

            return result;
        }

        public override void AddStatement(Statement statement)
        {
            throw new NotImplementedException();
        }

        public override void DeleteProject(string projectID)
        {
            throw new NotImplementedException();
        }

        public override void InitializeIdentificators()
        {
            throw new NotImplementedException();
        }

        public override void MoveNode(string nodeID, string newParentID, string newSiblingId)
        {
            throw new NotImplementedException();
        }

        public override Node UpdateHierarchy(Node hierarchyToUpdate, string parentID = null, string predecessorID = null)
        {
            throw new NotImplementedException();
        }

        public override void SaveIdentificators()
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

        public override Resource UpdateResource(Resource resource)
        {
            throw new NotImplementedException();
        }

        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
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
