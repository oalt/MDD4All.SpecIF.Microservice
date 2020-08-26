/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.MongoDB.DataAccess.Generic;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.Contracts.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts.Exceptions;
using System;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.MongoDB
{
	public class SpecIfMongoDbDataWriter : AbstractSpecIfDataWriter
	{
		private string DATABASE_NAME = "specif";

		private const string SPECIF_ADMIN_DATABASE_NAME = "specifAdmin";

		private MongoDBDataAccessor<Resource> _resourceMongoDbAccessor;
		private MongoDBDataAccessor<Statement> _statementMongoDbAccessor;
		private MongoDBDataAccessor<Node> _hierarchyMongoDbAccessor;
		private MongoDBDataAccessor<Node> _nodeMongoDbAccessor;
        private MongoDBDataAccessor<ProjectDescriptor> _projectMongoDbAccessor;

        private MongoDBDataAccessor<SpecIfIdentifier> _identifierMongoDbAccessor;

		public SpecIfMongoDbDataWriter(string connectionString, ISpecIfMetadataReader metadataReader, 
            ISpecIfDataReader dataReader, string dataBase = "specif") : base(metadataReader, dataReader)
		{
            if(dataBase != null)
            {
                DATABASE_NAME = dataBase;
            }

			_resourceMongoDbAccessor = new MongoDBDataAccessor<Resource>(connectionString, DATABASE_NAME);

			_statementMongoDbAccessor = new MongoDBDataAccessor<Statement>(connectionString, DATABASE_NAME);
			_hierarchyMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_nodeMongoDbAccessor = new MongoDBDataAccessor<Node>(connectionString, DATABASE_NAME);

			_identifierMongoDbAccessor = new MongoDBDataAccessor<SpecIfIdentifier>(connectionString, SPECIF_ADMIN_DATABASE_NAME);

            _projectMongoDbAccessor = new MongoDBDataAccessor<ProjectDescriptor>(connectionString, DATABASE_NAME);

            InitializeIdentificators();
		}

        public override void AddNodeAsFirstChild(string parentNodeId, Node newNode)
		{
            Node parentNode = _nodeMongoDbAccessor.GetItemWithLatestRevision(parentNodeId);

            if (parentNode != null)
            {
                if (string.IsNullOrEmpty(newNode.Revision))
                {
                    newNode.Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
                }

                if (string.IsNullOrEmpty(newNode.ID))
                {
                    newNode.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
                }
                
                _nodeMongoDbAccessor.Add(newNode);

                parentNode.NodeReferences.Insert(0, new Key(newNode.ID, newNode.Revision));

                _hierarchyMongoDbAccessor.Update(parentNode, parentNode.Id);

               
            }
            

        }

        public override void AddNodeAsPredecessor(string predecessorID, Node newNode)
        {
            Node parentNode = _dataReader.GetParentNode(new Key { ID = predecessorID, Revision = null });

            Node predecessor = _dataReader.GetNodeByKey(new Key { ID = predecessorID, Revision = null });

            if (parentNode != null && predecessor != null)
            {

                if (string.IsNullOrEmpty(newNode.Revision))
                {
                    newNode.Revision = SpecIfGuidGenerator.CreateNewRevsionGUID();
                }

                if (string.IsNullOrEmpty(newNode.ID))
                {
                    newNode.ID = SpecIfGuidGenerator.CreateNewSpecIfGUID();
                }

                int index = 0;

                foreach (Key nodeKey in parentNode.NodeReferences)
                {
                    if (nodeKey.ID == predecessor.ID)
                    {
                        break;
                    }
                    index++;
                }

                _nodeMongoDbAccessor.Add(newNode);

                parentNode.NodeReferences.Insert(index + 1, new Key(newNode.ID, newNode.Revision));

                _hierarchyMongoDbAccessor.Update(parentNode, parentNode.Id);


            }
        }

        public override Node UpdateHierarchy(Node hierarchyToUpdate, string parentID = null, string predecessorID = null)
        {

            Node result = hierarchyToUpdate;

            Node nodeToUpdate = _hierarchyMongoDbAccessor.GetItemById(hierarchyToUpdate.Id);

            if (nodeToUpdate != null)
            {
                hierarchyToUpdate.IsHierarchyRoot = nodeToUpdate.IsHierarchyRoot;
            }

            Node oldParent = _dataReader.GetParentNode(new Key { ID = hierarchyToUpdate.ID, Revision = null });



            if (parentID != null)
            {
                Node newParent = _dataReader.GetNodeByKey(new Key { ID = parentID, Revision = null });

                if (newParent != null)
                {

                    if (oldParent.Id == newParent.Id)
                    {
                        newParent = oldParent;
                    }

                    int removeIndex = 0;

                    foreach (Key nodeKey in oldParent.NodeReferences)
                    {
                        if (nodeKey.ID == nodeToUpdate.ID)
                        {
                            break;
                        }
                        removeIndex++;
                    }


                    oldParent.NodeReferences.RemoveAt(removeIndex);

                    // insert as first child at new parent
                    newParent.NodeReferences.Insert(0, new Key(nodeToUpdate.ID, nodeToUpdate.Revision));

                    _hierarchyMongoDbAccessor.Update(oldParent, oldParent.Id);

                    if (oldParent.Id != newParent.Id)
                    {
                        _hierarchyMongoDbAccessor.Update(newParent, newParent.Id);
                    }


                }
            }
            else if(predecessorID != null)
            {
                Node predecessor = _dataReader.GetNodeByKey(new Key { ID = predecessorID, Revision = null });

                if(predecessor != null)
                {
                    Node newParent = _dataReader.GetParentNode(new Key { ID = predecessor.ID, Revision = null });

                    if(newParent != null)
                    {
                        if (oldParent.Id == newParent.Id)
                        {
                            newParent = oldParent;
                        }

                        int removeIndex = 0;

                        foreach (Key nodeKey in oldParent.NodeReferences)
                        {
                            if (nodeKey.ID == nodeToUpdate.ID)
                            {
                                break;
                            }
                            removeIndex++;
                        }

                        oldParent.NodeReferences.RemoveAt(removeIndex);

                        int index = 0;

                        foreach (Key nodeKey in newParent.NodeReferences)
                        {
                            if (nodeKey.ID == predecessor.ID)
                            {
                                break;
                            }
                            index++;
                        }


                        newParent.NodeReferences.Insert(index + 1, new Key(nodeToUpdate.ID, nodeToUpdate.Revision));

                        _hierarchyMongoDbAccessor.Update(oldParent, oldParent.Id);

                        if (oldParent.Id != newParent.Id)
                        {
                            _hierarchyMongoDbAccessor.Update(newParent, newParent.Id);
                        }
                    }
                }
            }

            _hierarchyMongoDbAccessor.Update(hierarchyToUpdate, hierarchyToUpdate.Id);

            return result;
        }

        public override void MoveNode(string nodeID, string newParentID, string newSiblingId)
        {
            try
            {
                Node nodeToMove = _dataReader.GetNodeByKey(new Key { ID = nodeID, Revision = null });

                Node oldParent = _dataReader.GetParentNode(new Key { ID = nodeID, Revision = null });

                Node newParent = _dataReader.GetNodeByKey(new Key { ID = newParentID, Revision = null });

                if (oldParent.Id == newParent.Id)
                {
                    newParent = oldParent;
                }

                int removeIndex = 0;

                foreach (Key nodeKey in oldParent.NodeReferences)
                {
                    if (nodeKey.ID == nodeToMove.ID)
                    {
                        break;
                    }
                    removeIndex++;
                }

                oldParent.NodeReferences.RemoveAt(removeIndex);

                int insertIndex = 0;

                if (!string.IsNullOrEmpty(newSiblingId))
                {
                    foreach (Key nodeKey in newParent.NodeReferences)
                    {
                        insertIndex++;

                        if (nodeKey.ID == newSiblingId)
                        {
                            break;
                        }

                    }

                }

                newParent.NodeReferences.Insert(insertIndex, new Key(nodeToMove.ID, nodeToMove.Revision));

                UpdateHierarchy(oldParent);

                if (oldParent.Id != newParent.Id)
                {
                    UpdateHierarchy(newParent);
                }
                

            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public override void DeleteNode(string nodeID)
        {
            // we do not delete the element from the database, just remove the reference.

            Node nodeToDelete = _dataReader.GetNodeByKey(new Key { ID = nodeID, Revision = null });

            Node parent = _dataReader.GetParentNode(new Key { ID = nodeID, Revision = null });

            if(nodeToDelete != null && parent != null)
            {

                int index = -1;

                for (int counter = 0; counter < parent.NodeReferences.Count; counter++)
                {
                    
                    if(parent.NodeReferences[counter].ID == nodeID)
                    {
                        index = counter;
                        break;
                    }
                }

                if(index > -1)
                {
                    parent.NodeReferences.RemoveAt(index);
                }

                _hierarchyMongoDbAccessor.Update(parent, parent.Id);
            }
            else if(nodeToDelete != null && nodeToDelete.IsHierarchyRoot)
            {
                _hierarchyMongoDbAccessor.Delete(nodeToDelete.Id);
            }
        }

        public override void AddResource(Resource resource)
		{
            _resourceMongoDbAccessor.Add(resource);
        }

        public override void AddStatement(Statement statement)
		{
			_statementMongoDbAccessor.Add(statement);
		}

		public override void InitializeIdentificators()
		{
			SpecIfIdentifiers identifiers = new SpecIfIdentifiers();

			identifiers.Identifiers = new List<SpecIfIdentifier>(_identifierMongoDbAccessor.GetItems());

			_identificators = identifiers;
		}

		public override void SaveIdentificators()
		{
			foreach (SpecIfIdentifier identifier in _identificators.Identifiers)
			{
				_identifierMongoDbAccessor.Update(identifier, identifier.Id);
			}
		}

        public override void AddHierarchy(Node hierarchy, string projectID = null)
		{
            hierarchy.IsHierarchyRoot = true;

            if (projectID == null)
            {
                hierarchy.ProjectID = DEFAULT_PROJECT;
            }
            else
            {
                hierarchy.ProjectID = projectID;
            }

            AddHierarchyRecusrsively(hierarchy);
		}

        private void AddHierarchyRecusrsively(Node currentNode)
        {
            _hierarchyMongoDbAccessor.Add(currentNode);

            foreach(Node childNode in currentNode.Nodes)
            {
                AddHierarchyRecusrsively(childNode);
            }
        }

		

        public override Resource SaveResource(Resource resource, string projectID = null)
		{
            Resource result = null;

            Resource newResource = UpdateVersionInfo<Resource>(resource) as Resource;

            if (newResource != null)
            {
                if (projectID == null)
                {
                    newResource.ProjectID = DEFAULT_PROJECT;
                }
                else
                {
                    resource.ProjectID = projectID;
                }

                AddResource(newResource);
                result = newResource;
            }
            else
            {
                result = null;
            }

            return result;

        }

        public override Resource UpdateResource(Resource resource)
        {
            Resource result = null;

            Resource newResource = UpdateVersionInfo<Resource>(resource, true) as Resource;

            if (newResource != null)
            {
                AddResource(newResource);
                result = newResource;
            }
            else
            {
                result = null;
            }

            return result;
        }

        public override Statement SaveStatement(Statement statement, string projectID = null)
		{
            Statement result = null;

            Statement newStatement = UpdateVersionInfo<Statement>(statement) as Statement;

            if (newStatement != null)
            {
                newStatement.ProjectID = projectID;
                AddStatement(newStatement);
                result = newStatement;
            }
            else
            {
                result = null;
            }

            return result;
        }

        protected override IdentifiableElement GetItemWithLatestRevisionInBranch<T>(string id, string branch)
        {
            IdentifiableElement result = null;

            if(typeof(T) == typeof(Resource))
            {
                result = _resourceMongoDbAccessor.GetItemWithLatestRevisionInBranch(id, branch);
            }
            else if(typeof(T) == typeof(Statement))
            {
                result = _statementMongoDbAccessor.GetItemWithLatestRevisionInBranch(id, branch);
            }

            return result;
        }

        private const string DEFAULT_PROJECT = "PRJ-DEFAULT";

        public override void AddProject(ISpecIfMetadataWriter metadataWriter,
                                        DataModels.SpecIF specif, 
                                        string integrationID = null)
        {

            string projectID = DEFAULT_PROJECT;

            if(integrationID == null)
            {
                ProjectDescriptor defaultProject = _projectMongoDbAccessor.GetItemById(DEFAULT_PROJECT);

                if (defaultProject == null)
                {
                    defaultProject = new ProjectDescriptor()
                    {
                        CreatedAt = DateTime.Now,
                        Description = "SpecIF default project.",
                        ID = DEFAULT_PROJECT,
                        Title = "Default Project"

                    };

                    _projectMongoDbAccessor.Add(defaultProject);
                }
            }
            else
            {
                ProjectDescriptor integrationProject = _projectMongoDbAccessor.GetItemById(integrationID);

                if(integrationProject == null)
                {
                    specif.ID = integrationID;

                    _projectMongoDbAccessor.Add(new ProjectDescriptor(specif));

                    projectID = integrationID;
                }
            }

            

            IntegrateProjectData(metadataWriter, specif, projectID);
            
        }

        public override void UpdateProject(ISpecIfMetadataWriter metadataWriter, 
                                           DataModels.SpecIF project)
        {
            ProjectDescriptor integrationProject = _projectMongoDbAccessor.GetItemById(project.ID);

            if(integrationProject != null)
            {
                _projectMongoDbAccessor.Update(new ProjectDescriptor(project), project.ID);

                IntegrateProjectData(metadataWriter, project, project.ID);
            }
            else
            {
                throw new ProjectNotFoundException();
            }

        }

        public override void DeleteProject(string projectID)
        {
            throw new NotImplementedException();
        }

        
    }
}
