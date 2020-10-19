/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.Exceptions;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbDataReader : AbstractSpecIfDataReader
	{
		private string DATABASE_NAME = "specif";

		private MongoDBDataAccessor<Resource> _resourceMongoDbAccessor;
		private MongoDBDataAccessor<Statement> _statementMongoDbAccessor;
		private MongoDBDataAccessor<Node> _hierarchyMongoDbAccessor;
		private MongoDBDataAccessor<Node> _nodeMongoDbAccessor;
        private MongoDBDataAccessor<ProjectDescriptor> _projectMongoDbAccessor;



        public SpecIfMongoDbDataReader(string connectionString, string dataBase = "specif")
		{
            if (dataBase != null)
            {
                DATABASE_NAME = dataBase;
            }

            _resourceMongoDbAccessor = new MongoDBDataAccessor<Resource>(connectionString, DATABASE_NAME);

			_statementMongoDbAccessor = new MongoDBDataAccessor<Statement>(connectionString, DATABASE_NAME);
			_hierarchyMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_nodeMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

            _projectMongoDbAccessor = new MongoDBDataAccessor<ProjectDescriptor>(connectionString, DATABASE_NAME);

        }

        public override List<Node> GetAllHierarchyRootNodes(string projectID = null)
        {
            List<Node> result = new List<Node>();

            BsonDocument filter;

            if (projectID == null)
            {
                filter = new BsonDocument()
                {
                    { "isHierarchyRoot", true }
                };
            }
            else
            {
                filter = new BsonDocument()
                {
                    { "isHierarchyRoot", true },
                    { "project", projectID }
                };
            }

            result = new List<Node>(_hierarchyMongoDbAccessor.GetItemsByFilter(filter.ToJson()));

            return result;

        }

        public override List<Node> GetAllHierarchies()
		{
			List<Node> result = new List<Node>();

            result = GetAllHierarchyRootNodes();

			foreach(Node rootNode in result)
			{
				ResolveNodesRecursively(rootNode);
			}

			return result;
		}

		public override byte[] GetFile(string filename)
		{
			// TODO Use mongodb for files
			string path = @"C:\specif\files_and_images\" + filename;

			byte[] result = System.IO.File.ReadAllBytes(path);

			return result;
		}

		public override Node GetHierarchyByKey(Key key)
		{
			Node result = null;

            result = GetNodeByKey(key);

            if (result != null)
            {
                ResolveNodesRecursively(result);
            }

			return result;
		}

        public override Node GetNodeByKey(Key key)
        {
            Node result = null;

            if (key.Revision != null)
            {
                result = _hierarchyMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
            }
            else
            {
                result = _hierarchyMongoDbAccessor.GetItemByFilter("{ 'id' : '" + key.ID + "'}");
            }

            return result;
        }

		private void ResolveNodesRecursively(Node parent)
		{
			foreach (Key nodeKey in parent.NodeReferences)
			{
				if (parent.Nodes == null)
				{
					parent.Nodes = new List<Node>();
				}

				Node childNode = _nodeMongoDbAccessor.GetItemById(nodeKey.ID + "_R_" + nodeKey.Revision);

				parent.Nodes.Add(childNode);

				ResolveNodesRecursively(childNode);
			}

		}

		public override string GetLatestResourceRevisionForBranch(string resourceID, string branchName)
		{
			Resource resource = _resourceMongoDbAccessor.GetItemWithLatestRevisionInBranch(resourceID, branchName);

			return resource.Revision;
		}

		

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			
			result = _resourceMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			
			return result;
		}

		public override Statement GetStatementByKey(Key key)
		{
			Statement result = null;

			result = _statementMongoDbAccessor.GetItemById(key.ID + "_R_" + key.Revision);
			
			return result;
		}

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			Node hierarchy = _hierarchyMongoDbAccessor.GetItemWithLatestRevision(hierarchyID);

			return hierarchy.Revision;
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			Statement statement = _statementMongoDbAccessor.GetItemWithLatestRevision(statementID);

			return statement.Revision;
				 
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
			List<Statement> result = new List<Statement>();

			BsonDocument filter = new BsonDocument()
			{
				{ "$or", new BsonArray()
						 {
							new BsonDocument()
							{
								{ "subject.id", resourceKey.ID }
							},
							new BsonDocument()
							{
								{ "object.id", resourceKey.ID }
							}
						}
				}
			};

			result = _statementMongoDbAccessor.GetItemsByFilter(filter.ToJson());

			return result;
		}

		public override List<Node> GetContainingHierarchyRoots(Key resourceKey)
		{
			List<Node> result = new List<Node>();

			BsonDocument filter = new BsonDocument()
			{
				{ "resource.id" , resourceKey.ID  },
				{ "resource.revision", resourceKey.Revision }
			};

			List<Node> searchResult = _hierarchyMongoDbAccessor.GetItemsByFilter(filter.ToJson());

			if(searchResult != null)
			{
				List<Node> rootNodes = new List<Node>();

				foreach(Node node in searchResult)
				{
					FindRootNode(node, rootNodes);
				}

				if(rootNodes != null)
				{
					foreach(Node rootNode in rootNodes)
					{
						result.Add(rootNode);
					}
				}
			}

			return result; 
		}

		private void FindRootNode(Node currentNode, List<Node> result)
		{
			if(!currentNode.IsHierarchyRoot)
			{
				BsonDocument filter = new BsonDocument()
				{
					
					{ "nodes.id" , currentNode.ID },
							
					{ "nodes.revision", currentNode.Revision },
					
				};


				List<Node> searchResult = _hierarchyMongoDbAccessor.GetItemsByFilter(filter.ToJson());

				if (searchResult != null)
				{
					foreach(Node node in searchResult)
					{
						FindRootNode(node, result);
					}
				}

			}
			else
			{
				result.Add(currentNode);
			}
		}

        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            List<Resource> result = new List<Resource>();
            
            BsonDocument filter = new BsonDocument()
            {
                {  "id", resourceID }
            };

            List<Resource> dbResult = _resourceMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if(dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<Statement> GetAllStatements()
        {
            List<Statement> result = new List<Statement>();

            List<Statement> dbResult = _statementMongoDbAccessor.GetItems().ToList();

            if(dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            List<Statement> result = new List<Statement>();

            BsonDocument filter = new BsonDocument()
            {
                {  "id", statementID }
            };

            List<Statement> dbResult = _statementMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if (dbResult != null)
            {
                result = dbResult;
            }

            return result;
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
        {
            List<Node> result = new List<Node>();

            Node parentNode = GetNodeByKey(parentNodeKey);

            if(parentNode != null)
            {
                foreach(Key childKey in parentNode.NodeReferences)
                {
                    Node childNode = GetNodeByKey(childKey);
                    if(childNode != null)
                    {
                        result.Add(childNode);
                    }
                    else
                    {
                        result = null;
                        break;
                    }
                }
            }
            else
            {
                result = null;
            }

            return result;
        }

        public override Node GetParentNode(Key childNodeKey)
        {
            Node result = null;

            BsonDocument filter;

            if (childNodeKey.Revision != null)
            {
                filter = new BsonDocument()
                {

                    { "nodes.id" , childNodeKey.ID },

                    { "nodes.revision", childNodeKey.Revision },


                };
            }
            else
            {
                filter = new BsonDocument()
                {
                    { "nodes.id" , childNodeKey.ID }   
                };
            }

            List<Node> searchResult = _nodeMongoDbAccessor.GetItemsByFilter(filter.ToJson());

            if(searchResult != null && searchResult.Count == 1)
            {
                result = searchResult[0];
            }

            return result;
        }

        public override DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, 
                                                     string projectID, 
                                                     List<Key> hierarchyFilter = null, 
                                                     bool includeMetadata = true)
        {
            DataModels.SpecIF result = new DataModels.SpecIF();

            ProjectDescriptor project = _projectMongoDbAccessor.GetItemById(projectID);

            if(project == null)
            {
                throw new ProjectNotFoundException();
            }
            else
            {
                result.ID = project.ID;
                result.Title = project.Title;
                result.Description = project.Description;
                //result.Schema = project.Schema;
                result.Generator = project.Generator;
                result.GeneratorVersion = project.GeneratorVersion;
                result.CreatedAt = DateTime.Now;
                result.CreatedBy = project.CreatedBy;
                result.Rights = project.Rights;

                Dictionary<Key, Node> hierarchies = new Dictionary<Key, Node>();
                Dictionary<Key, Resource> resources = new Dictionary<Key, Resource>();
                Dictionary<Key, Statement> statements = new Dictionary<Key, Statement>();

                Dictionary<Key, ResourceClass> resourceClasses = new Dictionary<Key, ResourceClass>();
                Dictionary<Key, StatementClass> statementClasses = new Dictionary<Key, StatementClass>();
                Dictionary<Key, DataType> dataTypes = new Dictionary<Key, DataType>();
                Dictionary<Key, PropertyClass> propertyClasses = new Dictionary<Key, PropertyClass>();

                List<Node> hierarchyRoots = GetAllHierarchyRootNodes(projectID);

                if (hierarchyFilter != null && hierarchyFilter.Count > 0)
                {
                    foreach(Node hierarchyRoot in hierarchyRoots)
                    {
                        if(hierarchyFilter.Contains(new Key(hierarchyRoot.ID, hierarchyRoot.Revision)))
                        {
                            Key hierarchyKey = new Key(hierarchyRoot.ID, hierarchyRoot.Revision);
                            Node hierarchy = GetHierarchyByKey(hierarchyKey);

                            if(!hierarchies.ContainsKey(hierarchyKey))
                            {
                                hierarchies.Add(hierarchyKey, hierarchy);
                            }

                        }
                    }
                }
                else
                {
                    foreach (Node hierarchyRoot in hierarchyRoots)
                    {
                        Key hierarchyKey = new Key(hierarchyRoot.ID, hierarchyRoot.Revision);
                        Node hierarchy = GetHierarchyByKey(hierarchyKey);

                        if (!hierarchies.ContainsKey(hierarchyKey))
                        {
                            hierarchies.Add(hierarchyKey, hierarchy);
                        }

                    }
                }

                foreach (KeyValuePair<Key, Node> hierarchyNode in hierarchies)
                {
                    AddResourcesRecursively(hierarchyNode.Value, ref resources);
                }

                //TODO: Statements

                foreach(KeyValuePair<Key, Resource> cachedResource in resources)
                {
                    result.Resources.Add(cachedResource.Value);
                }

                foreach (KeyValuePair<Key, Node> cachedHierarchy in hierarchies)
                {
                    result.Hierarchies.Add(cachedHierarchy.Value);
                }

                if (includeMetadata)
                {
                    foreach (KeyValuePair<Key, Resource> resourceKeyValuePair in resources)
                    {
                        Resource resource = resourceKeyValuePair.Value;

                        foreach (Property property in resource.Properties)
                        {
                            if (!propertyClasses.ContainsKey(property.PropertyClass))
                            {
                                PropertyClass propertyClass = metadataReader.GetPropertyClassByKey(property.PropertyClass);
                                if(propertyClass != null)
                                {
                                    propertyClasses.Add(property.PropertyClass, propertyClass);

                                    if(!dataTypes.ContainsKey(propertyClass.DataType))
                                    {
                                        DataType dataType = metadataReader.GetDataTypeByKey(propertyClass.DataType);
                                        if(dataType != null)
                                        {
                                            dataTypes.Add(propertyClass.DataType, dataType);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach(KeyValuePair<Key, PropertyClass> cachedPropertyClass in propertyClasses)
                    {
                        result.PropertyClasses.Add(cachedPropertyClass.Value);
                    }

                    foreach (KeyValuePair<Key, DataType> cachedDataType in dataTypes)
                    {
                        result.DataTypes.Add(cachedDataType.Value);
                    }

                    

                } // if(includeMetadata)
            }


            return result;
        }

        private void AddResourcesRecursively(Node currentNode, ref Dictionary<Key, Resource> result)
        {
            Resource resource = GetResourceByKey(currentNode.ResourceReference);

            if(!result.ContainsKey(currentNode.ResourceReference))
            {
                result.Add(currentNode.ResourceReference, resource);
            }

            foreach(Node childNode in currentNode.Nodes)
            {
                AddResourcesRecursively(childNode, ref result);
            }
        }

        public override List<ProjectDescriptor> GetProjectDescriptions()
        {
            List<ProjectDescriptor> result = new List<ProjectDescriptor>();

            result = _projectMongoDbAccessor.GetItems().ToList();

            return result;
        }
    }
}
