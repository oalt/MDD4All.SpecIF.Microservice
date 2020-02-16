/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileDataReader : AbstractSpecIfDataReader
	{
		private DataModels.SpecIF _specIfData;

		public override List<Node> GetAllHierarchies()
		{
			List<Node> result = new List<Node>();

			List<Node> allHierarchiyNodes = _specIfData?.Hierarchies;

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

			return result;
		}

		public override Node GetHierarchyByKey(Key key)
		{
			Node result = null;

			List<Node> hierarchiesWithSameID = _specIfData?.Hierarchies.FindAll(res => res.ID == key.ID);

			if (hierarchiesWithSameID.Count != 0)
			{
				
				result = hierarchiesWithSameID.Find(r => r.Revision == key.Revision);
				
			}

			return result;
		}

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			List<Resource> resourcesWithSameID = _specIfData?.Resources.FindAll(res => res.ID == key.ID);

			if (resourcesWithSameID.Count != 0)
			{
				
				result = resourcesWithSameID.Find(r => r.Revision == key.Revision);
				
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

			List<Statement> statementsWithSameID = _specIfData?.Statements.FindAll(res => res.ID == key.ID);

			if (statementsWithSameID.Count != 0)
			{
				
				result = statementsWithSameID.Find(r => r.Revision == key.Revision);
				
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

        public override List<Node> GetAllHierarchyRootNodes()
        {
            throw new NotImplementedException();
        }

        public override List<Resource> GetAllResourceRevisions(string resourceID)
        {
            throw new NotImplementedException();
        }

        public override List<Statement> GetAllStatements()
        {
            return _specIfData?.Statements;
        }

        public override List<Statement> GetAllStatementRevisions(string statementID)
        {
            throw new NotImplementedException();
        }

        public override List<Node> GetChildNodes(Key parentNodeKey)
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
    }
}
