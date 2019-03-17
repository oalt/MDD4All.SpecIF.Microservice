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

		public override List<Hierarchy> GetAllHierarchies()
		{
			return _specIfData?.Hierarchies;
		}

		public override Hierarchy GetHierarchyByKey(Key key)
		{
			Hierarchy result = null;

			List<Hierarchy> hierarchiesWithSameID = _specIfData?.Hierarchies.FindAll(res => res.ID == key.ID);

			if (hierarchiesWithSameID.Count != 0)
			{
				if (key.Revision == 0)
				{
					result = hierarchiesWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = hierarchiesWithSameID.Find(r => r.Revision == key.Revision);
				}
			}

			return result;
		}

		public override Resource GetResourceByKey(Key key)
		{
			Resource result = null;

			List<Resource> resourcesWithSameID = _specIfData?.Resources.FindAll(res => res.ID == key.ID);

			if (resourcesWithSameID.Count != 0)
			{
				if (key.Revision == 0)
				{
					result = resourcesWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = resourcesWithSameID.Find(r => r.Revision == key.Revision);
				}
			}

			return result;
		}

		public override int GetLatestResourceRevision(string resourceID)
		{
			int result = 0;

			try
			{
				int? latestRevision = _specIfData?.Resources.FindAll(res => res.ID == resourceID).OrderByDescending(r => r.Revision).First().Revision;

				if (latestRevision.HasValue)
				{
					result = latestRevision.Value;
				}
			}
			catch (Exception)
			{
				;
			}

			return result;
		}

		public override Statement GetStatementByKey(Key key)
		{
			Statement result = null;

			List<Statement> statementsWithSameID = _specIfData?.Statements.FindAll(res => res.ID == key.ID);

			if (statementsWithSameID.Count != 0)
			{
				if (key.Revision == 0)
				{
					result = statementsWithSameID.OrderByDescending(r => r.Revision).First();
				}
				else
				{
					result = statementsWithSameID.Find(r => r.Revision == key.Revision);
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

		public override int GetLatestHierarchyRevision(string hierarchyID)
		{
			int result = 0;

			try
			{
				int? latestRevision = _specIfData?.Hierarchies.FindAll(res => res.ID == hierarchyID).OrderByDescending(r => r.Revision).First().Revision;

				if (latestRevision.HasValue)
				{
					result = latestRevision.Value;
				}
			}
			catch (Exception)
			{
				;
			}

			return result;
		}

		public override int GetLatestStatementRevision(string statementID)
		{
			int result = 0;

			try
			{
				int? latestRevision = _specIfData?.Statements.FindAll(res => res.ID == statementID).OrderByDescending(r => r.Revision).First().Revision;

				if (latestRevision.HasValue)
				{
					result = latestRevision.Value;
				}
			}
			catch (Exception)
			{
				;
			}

			return result;
		}
	}
}
