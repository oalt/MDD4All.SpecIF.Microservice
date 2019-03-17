/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Diagnostics;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ResourceManipulationExtensions
    {
		public static string GetTypeName(this Resource resource, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			try
			{
				ResourceClass resourceType = dataProvider.GetResourceClassByKey(resource.ResourceClass);

				if (resourceType != null)
				{
					result = resourceType.Title;
				}
			}
			catch(Exception exception)
			{
				Debug.WriteLine("Error with getTypeName() " + resource.ResourceClass + exception);
			}
			return result;
		}

		public static ResourceClass GetResourceType(this Resource resource, ISpecIfMetadataReader dataProvider)
		{
			ResourceClass result = null;

			result = dataProvider.GetResourceClassByKey(resource.ResourceClass);
			
			return result;
		}

		public static void SetPropertyValue(this Resource resource, string propertyTitle, Value value, ISpecIfMetadataReader dataProvider)
		{
			bool propertyFound = false;

			foreach(Property property in resource.Properties)
			{
				if(property.Title == propertyTitle)
				{
					property.Value = value;
					propertyFound = true;
					break;
				}
			}

			if(!propertyFound)
			{
				ResourceClass resourceType = dataProvider.GetResourceClassByKey(resource.ResourceClass);

				if(resourceType != null)
				{
					PropertyClass matchingPropertyClass = null;
					Key matchingPropertyKey = null;


					foreach(Key propertyKey in resourceType.PropertyClasses)
					{
						PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(propertyKey);

						if(propertyClass != null)
						{
							if(propertyClass.Title == propertyTitle)
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

			foreach (Property property in resource.Properties)
			{
				if (property.Title == propertyTitle)
				{
					result = property.GetStringValue(dataProvider);
					break;
				}
			}

			return result;
		}
	}
}
