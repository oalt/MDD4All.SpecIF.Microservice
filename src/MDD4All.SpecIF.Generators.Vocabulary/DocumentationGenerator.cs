using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    public class DocumentationGenerator
    {

        private Dictionary<string, DataModels.SpecIF> _domainClasses = new Dictionary<string, DataModels.SpecIF>();

        private const string CRLF = "\r\n";

        private DataModels.SpecIF _metaDataSpecIF = new DataModels.SpecIF();

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

            foreach (KeyValuePair<string, DataModels.SpecIF> domain in _domainClasses)
            {
                result += GenerateDomainDocumentation(domain.Key, domain.Value);
            }

            return result;
        }

        private string GenerateDomainDocumentation(string key, DataModels.SpecIF domainClasses)
        {
            string result = "";

            string domainName = key.Replace("_", ": ");

            result += "## Domain " + domainName + Environment.NewLine;

            result += Environment.NewLine;

            // data types
            if (domainClasses.DataTypes != null && domainClasses.DataTypes.Count != 0)
            {

                result += "### Data types of domain " + domainName + Environment.NewLine;

                result += "|title|id|revision|type|description|" + Environment.NewLine;

                result += "|-|-|-|-|" + Environment.NewLine;

                foreach (DataType dataType in domainClasses.DataTypes)
                {
                    result += "|" + dataType.Title + "|" + dataType.ID + "|" + dataType.Revision;
                    result += "|" + dataType.Type;
                    result += "|" + GetDataTypeDescription(dataType) + Environment.NewLine;
                }
            }

            // properties
            if (domainClasses.PropertyClasses != null && domainClasses.PropertyClasses.Count != 0)
            {

                result += "### Property classes of domain " + domainName + Environment.NewLine;

                result += "|title|id|revision|dataType|description|" + Environment.NewLine;

                result += "|-|-|-|-|-|" + Environment.NewLine;

                foreach (PropertyClass propertyClass in domainClasses.PropertyClasses)
                {
                    result += "|" + propertyClass.Title + "|" + propertyClass.ID + "|" + propertyClass.Revision;
                    result += "|" + propertyClass.GetDataTypeTitle(_specIfMetadataReader);
                    
                    result += "|" + propertyClass.Description + Environment.NewLine;
                }
            }

            // resources
            if (domainClasses.ResourceClasses != null && domainClasses.ResourceClasses.Count != 0)
            {
                result += "### Resource classes of domain " + domainName + Environment.NewLine;

                result += "|title|id|revision|description|" + Environment.NewLine;

                result += "|-|-|-|-|" + Environment.NewLine;

                foreach (ResourceClass resourceClass in domainClasses.ResourceClasses)
                {
                    result += "|" + resourceClass.Title + "|" + resourceClass.ID + "|" + resourceClass.Revision;
                    result += "|" + GetResourceClassDescription(resourceClass) + Environment.NewLine;
                }
            }

            // statements
            if (domainClasses.StatementClasses != null && domainClasses.StatementClasses.Count != 0)
            {
                result += "### Statement classes of domain " + domainName + Environment.NewLine;

                result += "|title|id|revision|description|" + Environment.NewLine;

                result += "|-|-|-|-|" + Environment.NewLine;

                foreach (StatementClass statementClass in domainClasses.StatementClasses)
                {
                    result += "|" + statementClass.Title + "|" + statementClass.ID + "|" + statementClass.Revision;
                    result += "|" + GetStatementClassDescription(statementClass) + Environment.NewLine;
                }
            }

            return result;
        }

        private string GetDataTypeDescription(DataType dataType)
        {
            string result = "";

            if (dataType.Type == "xs:enumeration")
            {
                result = "<p>" + dataType.Description.ToString() + "</p>";
                
                if (dataType.Values != null)
                {
                    result += "<ul>";
                    foreach (EnumValue value in dataType.Values)
                    {
                        result += "<li>" + value.Title.ToString() + " [" + value.ID + "]</li>";
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
                    result = dataType.Description.ToString();
                }
            }

            return result;
        }

        private string GetResourceClassDescription(ResourceClass resourceClass)
        {
            string result = "";

            result += "<p>" + resourceClass.Description.ToString() + "</p>";

            if(resourceClass.PropertyClasses != null && resourceClass.PropertyClasses.Count != 0)
            {
                result += "<p>Property classes:<br/><ul>";

                foreach(Key key in resourceClass.PropertyClasses)
                {
                    result += "<li>" + key.GetPropertyClassTitle(_specIfMetadataReader) + "</li>";
                }

                result += "</ul></p>";
            }

            

            return result;
        }

        private string GetStatementClassDescription(StatementClass statementClass)
        {
            string result = "";

            result += "<p>" + statementClass.Description.ToString() + "</p>";

            if (statementClass.PropertyClasses != null && statementClass.PropertyClasses.Count != 0)
            {
                result += "<p>Property classes:<br/><ul>";

                foreach (Key key in statementClass.PropertyClasses)
                {
                    result += "<li>" + key.GetPropertyClassTitle(_specIfMetadataReader) + "</li>";
                }

                result += "</ul></p>";
            }

            return result;
        }

        private void InitializeClassDefinitions(DirectoryInfo domainDirectory)
        {
            string domainName = domainDirectory.Name;

            FileInfo[] specIfFiles = domainDirectory.GetFiles("*.specif");

            DataModels.SpecIF domainSpecIF = null;

            int fileConuter = 0;

            foreach (FileInfo fileInfo in specIfFiles)
            {
                fileConuter++;

                DataModels.SpecIF specIF = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(fileInfo.FullName);

                

                if (fileConuter == 1)
                {
                    domainSpecIF = specIF;
                }
                
                domainSpecIF.DataTypes.AddRange(specIF.DataTypes);
                domainSpecIF.PropertyClasses.AddRange(specIF.PropertyClasses);
                domainSpecIF.ResourceClasses.AddRange(specIF.ResourceClasses);
                domainSpecIF.StatementClasses.AddRange(specIF.StatementClasses);

                _metaDataSpecIF.DataTypes.AddRange(specIF.DataTypes);
                _metaDataSpecIF.PropertyClasses.AddRange(specIF.PropertyClasses);
                _metaDataSpecIF.ResourceClasses.AddRange(specIF.ResourceClasses);
                _metaDataSpecIF.StatementClasses.AddRange(specIF.StatementClasses);
            }

            if (domainSpecIF != null)
            {
                
                _domainClasses.Add(domainName, domainSpecIF);
            }


        }


    }

}