/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Linq;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class HierarchyManipulationExtensions
    {
		public static Node GetNodeById(this Node hierarchy, string id)
		{
			Node result = null;

			foreach(Node rootNode in hierarchy.Nodes)
			{
				FindNodeRecursively(rootNode, id, ref result);
				if(result != null)
				{
					break;
				}

			}

			return result;
		}

		private static void FindNodeRecursively(Node node, string id, ref Node result)
		{
			if(node.ID == id)
			{
				result = node;
			}
			else
			{
				if (node.Nodes != null)
				{
					foreach (Node child in node.Nodes)
					{
						FindNodeRecursively(child, id, ref result);
					}
				}
			}
		}

		//public static string GetResourceIdentifierPrefix(this Hierarchy hierarchy, ISpecIfMetadataReader dataProvider)
		//{
		//	string result = "";

		//	try
		//	{
		//		string prefix = hierarchy.Properties.FirstOrDefault(prop => prop.Title == "identifierPrefix")?.GetStringValue(dataProvider);

		//		if (prefix != null)
		//		{
		//			result = prefix;
		//		}
		//	}
		//	catch(Exception)
		//	{

		//	}

		//	return result;
		//}
	}
}
