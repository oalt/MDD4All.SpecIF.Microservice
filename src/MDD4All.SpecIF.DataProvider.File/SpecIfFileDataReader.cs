/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileDataReader : AbstractSpecIfDataReader
	{
        private string _dataRootPath;

		public Dictionary<string, DataModels.SpecIF> SpecIfData;

        public SpecIfFileDataReader(string dataRootPath)
        {
            _dataRootPath = dataRootPath;
            InitializeData();
        }

        private void InitializeData()
        {
            SpecIfData = new Dictionary<string, DataModels.SpecIF>();

            InitializeDataRecusrsively(_dataRootPath);
        }

        private void InitializeDataRecusrsively(string currentPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(currentPath);
            FileInfo[] specifFiles = directoryInfo.GetFiles("*.specif");

            foreach (FileInfo fileInfo in specifFiles)
            {
                DataModels.SpecIF currentSepcIF = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(fileInfo.FullName);

                if(!SpecIfData.ContainsKey(fileInfo.FullName))
                {
                    SpecIfData.Add(fileInfo.FullName, currentSepcIF);
                }
                
            }

            foreach (DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                InitializeDataRecusrsively(subDirectoryInfo.FullName);
            }



        }


        public override List<Node> GetAllHierarchies()
		{
			List<Node> result = new List<Node>();

            foreach(KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                List<Node> allHierarchiyNodes = keyValuePair.Value?.Hierarchies;

                foreach (Node node in allHierarchiyNodes)
                {
                    bool predecessorFound = false;
                    foreach (Node n in allHierarchiyNodes)
                    {
                        if (n.ResourceReference.ID == node.ID && n.ResourceReference.Revision == node.Revision)
                        {
                            predecessorFound = true;
                            break;
                        }
                    }
                    if (predecessorFound == false)
                    {
                        result.Add(node);
                    }
                }
            }
			

			return result;
		}

		public override Node GetHierarchyByKey(Key key)
		{
			Node result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                List<Node> hierarchiesWithSameID = keyValuePair.Value?.Hierarchies.FindAll(res => res.ID == key.ID);

                if (hierarchiesWithSameID.Count != 0 )
                {
                    if (key.Revision != null)
                    {
                        result = hierarchiesWithSameID.Find(r => r.Revision == key.Revision);
                        break;
                    }
                    else
                    {
                        result = hierarchiesWithSameID[0];
                        break;
                    }
                    
                }
            }

			return result;
		}

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
               Resource resource = keyValuePair.Value?.Resources.Find(res => res.ID == key.ID && res.Revision == key.Revision);

                if(resource != null)
                {
                    result = resource;
                    break;
                }
            }

			return result;
		}

		public override string GetLatestResourceRevisionForBranch(string resourceID, string branchName)
		{
			string result = null;

			// TODO
			//try
			//{
			//	string? latestRevision = _specIfData?.Resources.FindAll(res => res.ID == resourceID).OrderByDescending(r => r.Revision).First().Revision;

			//	if (latestRevision.HasValue)
			//	{
			//		result = latestRevision.Value;
			//	}
			//}
			//catch (Exception)
			//{
			//	;
			//}

			return result;
		}

		public override Statement GetStatementByKey(Key key)
		{
			Statement result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                Statement statement = keyValuePair.Value?.Statements.Find(res => res.ID == key.ID && res.Revision == key.Revision);

                if (statement!=null)
                {
                    result = statement;
                    break;
                }
                
            }

			return result;
		}

		public override byte[] GetFile(string filename)
		{
			string path = @"C:\specif\files_and_images\" + filename;

			byte[] result = System.IO.File.ReadAllBytes(path);

			return result;
		}

		public override string GetLatestHierarchyRevision(string hierarchyID)
		{
			string result = null;

			//try
			//{
			//	int? latestRevision = _specIfData?.Hierarchies.FindAll(res => res.ID == hierarchyID).OrderByDescending(r => r.Revision).First().Revision;

			//	if (latestRevision.HasValue)
			//	{
			//		result = latestRevision.Value;
			//	}
			//}
			//catch (Exception)
			//{
			//	;
			//}

			return result;
		}

		public override string GetLatestStatementRevision(string statementID)
		{
			string result = null;

			//try
			//{
			//	int? latestRevision = _specIfData?.Statements.FindAll(res => res.ID == statementID).OrderByDescending(r => r.Revision).First().Revision;

			//	if (latestRevision.HasValue)
			//	{
			//		result = latestRevision.Value;
			//	}
			//}
			//catch (Exception)
			//{
			//	;
			//}

			return result;
		}

		public override List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
			throw new NotImplementedException();
		}

		public override List<Node> GetContainingHierarchyRoots(Key resourceKey)
		{
			throw new NotImplementedException();
		}

        public override List<Node> GetAllHierarchyRootNodes(string projectID = null)
        {
            List<Node> result = new List<Node>();

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                DataModels.SpecIF specIF = keyValuePair.Value;

                if(projectID != null)
                {
                    if(specIF.ID == projectID)
                    {
                        result.AddRange(specIF.Hierarchies);
                        break;
                    }
                }
                else
                {
                    result.AddRange(specIF.Hierarchies);
                }
            }

            // delete child nodes in result
            //foreach(Node rootNode in result)
            //{
            //    rootNode.Nodes = new List<Node>();
            //}

            return result;
        }

        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatements()
        {
            List<Statement> result = new List<Statement>();

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                result.AddRange(keyValuePair.Value.Statements);
            }

            return result;
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            throw new NotImplementedException();
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
        {
            List<Node> result = new List<Node>();

            foreach(KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                DataModels.SpecIF specif = keyValuePair.Value;

                foreach(Node hierarchy in specif.Hierarchies)
                {
                    Node parentNode = null;
                    FindNodeRecusrsively(hierarchy, parentNodeKey, ref parentNode);

                    if(parentNode != null)
                    {
                        result.AddRange(parentNode.Nodes);
                        break;
                    }
                }
            }

            return result;
        }

        public override Node GetNodeByKey(Key key)
        {
            Node result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                DataModels.SpecIF specif = keyValuePair.Value;

                foreach (Node hierarchy in specif.Hierarchies)
                {
                    Node parentNode = null;
                    FindNodeRecusrsively(hierarchy, key, ref parentNode);

                    if (parentNode != null)
                    {
                        result = parentNode;
                        break;
                    }
                }
            }

            return result;
        }

        public override Node GetParentNode(Key childNode)
        {
            Node result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                DataModels.SpecIF specif = keyValuePair.Value;

                foreach (Node hierarchy in specif.Hierarchies)
                {

                    FindParentNodeRecusrsively(hierarchy, childNode, ref result);
                    if(result != null)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        private void FindNodeRecusrsively(Node currentNode, Key key, ref Node result)
        {
            if(currentNode.ID == key.ID && currentNode.Revision == key.Revision)
            {
                result = currentNode;
            }
            else
            {
                foreach(Node childNode in currentNode.Nodes)
                {
                    FindNodeRecusrsively(childNode, key, ref result);
                }
            }
        }

        private void FindParentNodeRecusrsively(Node currentNode, Key key, ref Node result)
        {
            foreach (Node childNode in currentNode.Nodes)
            {
                if (childNode.ID == key.ID && childNode.Revision == key.Revision)
                {
                    result = currentNode;
                    break;
                }
            }
            
            if(result == null)
            {
                foreach (Node childNode in currentNode.Nodes)
                {
                    FindParentNodeRecusrsively(childNode, key, ref result);
                }
            }
            
        }

        public override DataModels.SpecIF GetProject(ISpecIfMetadataReader metadataReader, string projectID,
            List<Key> hierarchyFilter = null, bool includeMetadata = true)
        {
            DataModels.SpecIF result = null;

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                if (keyValuePair.Value.ID == projectID)
                {
                    result = keyValuePair.Value;
                    break;
                }
            }

            return result;
        }

        public override List<ProjectDescriptor> GetProjectDescriptions()
        {
            List<ProjectDescriptor> result = new List<ProjectDescriptor>();

            foreach (KeyValuePair<string, DataModels.SpecIF> keyValuePair in SpecIfData)
            {
                ProjectDescriptor projectDescriptor = new ProjectDescriptor(keyValuePair.Value);
            }

            return result;
        }
    }
}
