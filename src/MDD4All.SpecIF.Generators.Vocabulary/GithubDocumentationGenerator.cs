using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    public class GithibDocumentationGenerator
    {

        private Dictionary<string, SpecIF.DataModels.SpecIF> _domainClasses = new Dictionary<string, SpecIF.DataModels.SpecIF>();

        private const string CRLF = "\r\n";

        private SpecIF.DataModels.SpecIF _metaDataSpecIF = new SpecIF.DataModels.SpecIF();

        private ISpecIfMetadataReader _specIfMetadataReader;

        public string GenerateVocabularyDocumentation(string[] classDefinitionRoot)
        {
            string result = "";

            // read all datatype and class definition
            foreach (string path in classDefinitionRoot)
            {
                DirectoryInfo classDefinitionRootDirectory = new DirectoryInfo(path);

                foreach (DirectoryInfo domainDirectoryInfo in classDefinitionRootDirectory.GetDirectories())
                {
                    if (domainDirectoryInfo.Name.StartsWith("01") ||
                        domainDirectoryInfo.Name.StartsWith("02") ||
                        domainDirectoryInfo.Name.StartsWith("03"))
                    {

                        InitializeClassDefinitions(domainDirectoryInfo);
                    }
                }

            }

            _specIfMetadataReader = new SpecIfFileMetadataReader(_metaDataSpecIF);

            foreach (KeyValuePair<string, SpecIF.DataModels.SpecIF> domain in _domainClasses)
            {
                result += GenerateDomainDocumentation(domain.Key, domain.Value);
            }

            return result;
        }

        private string GenerateDomainDocumentation(string key, SpecIF.DataModels.SpecIF domainClasses)
        {
            string result = "";

            string domainName = key.Replace("_", ": ");

            result += "## Domain " + domainName + Environment.NewLine;

            result += Environment.NewLine;

            // data types
            if (domainClasses.DataTypes != null && domainClasses.DataTypes.Count != 0)
            {

                result += "### Data types of domain " + domainName + Environment.NewLine;

                result += Environment.NewLine;

                result += "|title|id|revision|type|description|" + Environment.NewLine;

                result += "|-|-|-|-|-|" + Environment.NewLine;

                foreach (DataType dataType in domainClasses.DataTypes)
                {
                    result += "|" + dataType.Title + "|" + dataType.ID + "|" + dataType.Revision;
                    result += "|" + dataType.Type;
                    result += "|" + GetDataTypeDescription(dataType) + "|" + Environment.NewLine;
                }
            }

            // properties
            if (domainClasses.PropertyClasses != null && domainClasses.PropertyClasses.Count != 0)
            {

                result += "### Property classes of domain " + domainName + Environment.NewLine;

                result += Environment.NewLine;

                result += "|title|id|revision|dataType|description|" + Environment.NewLine;

                result += "|-|-|-|-|-|" + Environment.NewLine;

                foreach (PropertyClass propertyClass in domainClasses.PropertyClasses)
                {
                    result += "|" + propertyClass.Title + "|" + propertyClass.ID + "|" + propertyClass.Revision;
                    result += "|" + propertyClass.GetDataTypeTitle(_specIfMetadataReader);

                    result += "|" + propertyClass.Description[0].Text + "|" + Environment.NewLine;
                }
            }

            // resources
            if (domainClasses.ResourceClasses != null && domainClasses.ResourceClasses.Count != 0)
            {
                result += "### Resource classes of domain " + domainName + Environment.NewLine;

                result += Environment.NewLine;

                result += "|title|id|revision|description|" + Environment.NewLine;

                result += "|-|-|-|-|" + Environment.NewLine;

                foreach (ResourceClass resourceClass in domainClasses.ResourceClasses)
                {
                    result += "|" + resourceClass.Title + "|" + resourceClass.ID + "|" + resourceClass.Revision;
                    result += "|" + GetResourceClassDescription(resourceClass) + "|" + Environment.NewLine;
                }
            }

            // statements
            if (domainClasses.StatementClasses != null && domainClasses.StatementClasses.Count != 0)
            {
                result += "### Statement classes of domain " + domainName + Environment.NewLine;

                result += Environment.NewLine;

                result += "|title|id|revision|description|" + Environment.NewLine;

                result += "|-|-|-|-|" + Environment.NewLine;

                foreach (StatementClass statementClass in domainClasses.StatementClasses)
                {
                    result += "|" + statementClass.Title + "|" + statementClass.ID + "|" + statementClass.Revision;
                    result += "|" + GetResourceClassDescription(statementClass) + "|" + Environment.NewLine;
                }
            }

            return result;
        }

        private string GetDataTypeDescription(DataType dataType)
        {
            string result = "";

            if (dataType.Type == "xs:enumeration")
            {
                result = "<p>" + dataType.Description[0].Text + "</p>";

                if (dataType.Values != null)
                {
                    result += "<ul>";
                    foreach (EnumerationValue value in dataType.Values)
                    {
                        result += "<li>" + value.Value[0].Text + " [" + value.ID + "]</li>";
                    }
                    result += "</ul>";
                }
            }
            else
            {
                if (dataType.Description.ToString() == "[]")
                {
                    result = "";
                }
                else
                {
                    result = dataType.Description[0].Text;
                }
            }

            return result;
        }

        private string GetResourceClassDescription(ResourceClass resourceClass)
        {
            string result = "";

            result += "<p>" + resourceClass.Description[0].Text + "</p>";

            if (resourceClass.PropertyClasses != null && resourceClass.PropertyClasses.Count != 0)
            {
                result += "<p>Property classes:<br/><ul>";

                foreach (Key key in resourceClass.PropertyClasses)
                {
                    PropertyClass propertyClass = _specIfMetadataReader.GetPropertyClassByKey(key);

                    result += "<li>" + key.GetPropertyClassTitle(_specIfMetadataReader) + " [" + propertyClass.ID + " " + propertyClass.Revision + "]</li>";
                }

                result += "</ul></p>";
            }



            return result;
        }

        private void InitializeClassDefinitions(DirectoryInfo domainDirectory)
        {
            string domainName = domainDirectory.Name;

            FileInfo[] specIfFiles = domainDirectory.GetFiles("*.specif");

            SpecIF.DataModels.SpecIF domainSpecIF = new SpecIF.DataModels.SpecIF();

            int fileConuter = 0;

            foreach (FileInfo fileInfo in specIfFiles)
            {
                fileConuter++;

                SpecIF.DataModels.SpecIF specIF = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(fileInfo.FullName);

                domainSpecIF.DataTypes.AddRange(specIF.DataTypes);
                domainSpecIF.PropertyClasses.AddRange(specIF.PropertyClasses);
                domainSpecIF.ResourceClasses.AddRange(specIF.ResourceClasses);
                domainSpecIF.StatementClasses.AddRange(specIF.StatementClasses);

                _metaDataSpecIF.DataTypes.AddRange(specIF.DataTypes);
                _metaDataSpecIF.PropertyClasses.AddRange(specIF.PropertyClasses);
                _metaDataSpecIF.ResourceClasses.AddRange(specIF.ResourceClasses);
                _metaDataSpecIF.StatementClasses.AddRange(specIF.StatementClasses);
            }

            _domainClasses.Add(domainName, domainSpecIF);

        }


    }

}