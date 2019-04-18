using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
	public static class StatementManipulationExtensions
	{
		public static string GetPropertyValue(this Statement statement, string propertyTitle, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			if (statement.Properties != null)
			{
				foreach (Property property in statement.Properties)
				{
					string title = property.Title.LanguageValues[0].Text;

					if (title == propertyTitle)
					{
						result = property.GetStringValue(dataProvider);
						break;
					}
				}
			}

			return result;
		}
	}
}
