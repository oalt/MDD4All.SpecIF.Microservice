/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataProvider.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class PropertyManipulationExtensions
    {
		public static string GetDataTypeType(this Property property, ISpecIfMetadataReader dataProvider)
		{
			string result = "";

			PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(property.Class);

			if (propertyClass != null)
			{
				DataType dataType = dataProvider.GetDataTypeByKey(propertyClass.DataType);
				result = dataType.Type;
			}
			return result;
		}

		public static DataType GetDataType(this Property property, ISpecIfMetadataReader dataProvider)
		{
			DataType result = null;

			PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(property.Class);

			if (propertyClass != null)
			{
				DataType dataType = dataProvider.GetDataTypeByKey(propertyClass.DataType);
				result = dataType;
			}
			return result;
		}

		public static string GetStringValue(this Property property, ISpecIfMetadataReader dataProvider, string language = "en")
		{
			string result = "";

			PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(property.Class);

			bool? isMultiple = null;

			if (propertyClass != null)
			{
				isMultiple = propertyClass.Multiple;
			}

			if (property.GetDataTypeType(dataProvider) == "xs:enumeration")
			{
				

				DataType enumDataType = property.GetDataType(dataProvider);

				if(enumDataType != null)
				{
					if (isMultiple != null && isMultiple == true)
					{
						//if (property.Value.LanguageValues?.FirstOrDefault() != null)
						//{
						//	char[] separator = { ',' };


						//	string[] values = property.Value.LanguageValues[0].Text.Split(separator);

						//	int counter = 0;

						//	foreach (string enumId in values)
						//	{
						//		EnumValue value = enumDataType.Values.Find(val => val.ID == enumId.Trim());

						//		if (value != null)
						//		{
						//			result += value.Title;

						//			if (counter < values.Length - 1)
						//			{
						//				result += ", ";
						//			}

						//		}
						//		counter++;
						//	}
						//}

					}
					else
					{
						if(property.Values.Count > 0)
                        {
							Value firstValue = property.Values[0];

							result = firstValue.ToString();
                        }


						
						//if (property.Value.LanguageValues?.FirstOrDefault() != null)
						//{
						//	string enumId = property.Value.LanguageValues[0].Text;
						//	EnumValue value = enumDataType.Values.Find(val => val.ID == enumId);

						//	if (value != null && value.Title != null)
						//	{
						//		result = value.Title.LanguageValues?.FirstOrDefault(val => val.Language == language)?.Text;

						//		if(result == null && value.Title.LanguageValues.Count > 0)
						//		{
						//			result = value.Title.LanguageValues[0].Text;
						//		}
						//	}
						//}

					}
				}
			}
			else
			{
				if (isMultiple != null && isMultiple == true)
				{
				}
				else
				{

					if (property.Values.Count > 0)
					{
						Value firstValue = property.Values[0];

						result = firstValue.ToString();
					}
					//LanguageValue languageValue = property.Value.LanguageValues.FirstOrDefault(val => val.Language == language);

					//if(languageValue == null)
					//{
					//	languageValue = property.Value.LanguageValues?.FirstOrDefault();
					//}

					//if (languageValue != null)
					//{
					//	result = languageValue.Text;
					//}
				}
			}
			return result;
		}
    }
}
