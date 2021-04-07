/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
	public static class ResourceManipulationExtensions
	{
		public static string GetTypeName(this Resource resource, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			try
			{
				ResourceClass resourceType = dataProvider.GetResourceClassByKey(resource.Class);

				if (resourceType != null)
				{
					if (resourceType.Title is string)
					{
						result = resourceType.Title.ToString();
					}
					//result = resourceType.Title.LanguageValues[0];
				}
			}
			catch (Exception exception)
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

		public static void SetPropertyValue(this Resource resource,
											string propertyTitle,
											string stringValue,
											ISpecIfMetadataReader metadataProvider,
											string format = "plain")
        {
			ResourceClass resourceClass = metadataProvider.GetResourceClassByKey(resource.Class);

			Value value = new Value();

			foreach(Key propertyClassKey in resourceClass.PropertyClasses)
            {
				PropertyClass propertyClass = metadataProvider.GetPropertyClassByKey(propertyClassKey);

				if(propertyClass.Title == propertyTitle)
                {
					DataType dataType = metadataProvider.GetDataTypeByKey(propertyClass.DataType);

					if(dataType.Type == "xs:string")
                    {
						MultilanguageText multilanguageText = new MultilanguageText
						{
							Text = stringValue,
							Format = format
						};

						value = new Value(multilanguageText);
                    }
					else
                    {
						value = new Value(stringValue);
                    }
                }
            }

			SetPropertyValue(resource, propertyTitle, value, metadataProvider);
        }

		public static void SetPropertyValue(this Resource resource, 
											string propertyTitle, 
											Value value, 
											ISpecIfMetadataReader metadataProvider)
		{
			bool propertyFound = false;

			foreach (Property property in resource.Properties)
			{

				PropertyClass propertyClass = metadataProvider.GetPropertyClassByKey(property.Class);

				if(propertyClass != null)
                {
					if(propertyClass.Title == propertyTitle)
                    {
						if (property.Values.Count == 0)
						{
							property.Values.Add(value);
						}
						else
						{
							property.Values[0] = value;
						}
						propertyFound = true;
						break;
                    }
                }

			}

			if (!propertyFound)
			{
				ResourceClass resourceType = metadataProvider.GetResourceClassByKey(resource.Class);

				if (resourceType != null)
				{
					PropertyClass matchingPropertyClass = null;
					Key matchingPropertyKey = null;


					foreach (Key propertyKey in resourceType.PropertyClasses)
					{
						PropertyClass propertyClass = metadataProvider.GetPropertyClassByKey(propertyKey);

						if (propertyClass.Title == propertyTitle)
						{
							matchingPropertyClass = propertyClass;
							matchingPropertyKey = propertyKey;
							break;
						}

					}

					if (matchingPropertyClass != null)
					{
						Property property = new Property()
						{
							Class = matchingPropertyKey,
							Values = new List<Value> { value }
						};

						resource.Properties.Add(property);
					}
				}


			}
		}

		public static void SetPropertyValue(this Resource resource, Key propertyClassKey, Value value)
		{
			bool propertyFound = false;

			foreach (Property property in resource.Properties)
			{
				if (property.Class.ID == propertyClassKey.ID && property.Class.Revision == propertyClassKey.Revision)
				{
					property.Values[0] = value;
					propertyFound = true;
					break;
				}
			}

			if (!propertyFound)
			{

				Property property = new Property()
				{
					
					Class = propertyClassKey,
					Values = new List<Value> { value }
				};

				resource.Properties.Add(property);
			}

		}
	

		public static string GetPropertyValue(this Resource resource, string propertyTitle, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			if (resource != null && resource.Properties != null)
			{
				foreach (Property property in resource.Properties)
				{
					PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(property.Class);

					if (propertyClass != null)
					{

						if (propertyClass.Title == propertyTitle)
						{
							result = property.GetStringValue(dataProvider);
							break;
						}
					}
				}
			}

			return result;
		}

		public static List<Value> GetPropertyValue(this Resource resource, Key propertyClassKey)
        {
			List<Value> result = new List<Value>();

			if (resource != null && resource.Properties != null)
			{
				foreach (Property property in resource.Properties)
				{
					

					if (propertyClassKey.ID == property.Class.ID && propertyClassKey.Revision == property.Class.Revision)
					{
						result = property.Values;
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
