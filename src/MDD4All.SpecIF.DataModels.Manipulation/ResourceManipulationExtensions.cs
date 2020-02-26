/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Diagnostics;
using System.Linq;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ResourceManipulationExtensions
    {
		public static LanguageValue GetTypeName(this Resource resource, ISpecIfMetadataReader dataProvider)
		{
			LanguageValue result = new LanguageValue();

			try
			{
				ResourceClass resourceType = dataProvider.GetResourceClassByKey(resource.Class);

				if (resourceType != null)
				{
					//result = resourceType.Title.LanguageValues[0];
				}
			}
			catch(Exception exception)
			{
				Debug.WriteLine("Error with getTypeName() " + exception);
			}
			return result;
		}

		public static ResourceClass GetResourceType(this Resource resource, ISpecIfMetadataReader dataProvider)
		{
			ResourceClass result = null;

			result = dataProvider.GetResourceClassByKey(resource.Class);
			
			return result;
		}

		public static void SetPropertyValue(this Resource resource, string propertyTitle, Value value, ISpecIfMetadataReader dataProvider)
		{
			bool propertyFound = false;

			foreach(Property property in resource.Properties)
			{
                string title = ""; // property.Title.LanguageValues.First(v => v.Text == propertyTitle).Text;

				if (title == propertyTitle)
				{
					property.Value = value;
					propertyFound = true;
					break;
				}
			}

			if(!propertyFound)
			{
				ResourceClass resourceType = dataProvider.GetResourceClassByKey(resource.Class);

				if(resourceType != null)
				{
					PropertyClass matchingPropertyClass = null;
					Key matchingPropertyKey = null;


					foreach(Key propertyKey in resourceType.PropertyClasses)
					{
						PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(propertyKey);

						if(propertyClass != null)
						{
                            string title = ""; // propertyClass.Title.LanguageValues[0].Text;

							if (title == propertyTitle)
							{
								matchingPropertyClass = propertyClass;
								matchingPropertyKey = propertyKey;
								break;
							}
						}
					}

					if(matchingPropertyClass != null)
					{
						Property property = new Property()
						{
							ID = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
							Description = matchingPropertyClass.Description,
							PropertyClass = matchingPropertyKey,
							Title = matchingPropertyClass.Title,
							Value = value
						};

						resource.Properties.Add(property);
					}
				}

				
			}
		}

		public static string GetPropertyValue(this Resource resource, string propertyTitle, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			if (resource != null && resource.Properties != null)
			{
				foreach (Property property in resource.Properties)
				{
                    string title = ""; // property.Title.LanguageValues[0].Text;

					if (title == propertyTitle)
					{
						result = property.GetStringValue(dataProvider);
						break;
					}
				}
			}

			return result;
		}

		public static void SetResourceTitle(this Resource resource, string title)
		{
			//if(resource.Title.LanguageValues.Count > 0)
			//{
			//	foreach(LanguageValue languageValue in resource.Title.LanguageValues)
			//	{
			//		languageValue.Text = title;
			//	}
			//}
			//else
			//{
			//	resource.Title.LanguageValues.Add(new LanguageValue(title));
			//}
		}
	}
}
