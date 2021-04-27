using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.Generators.Vocabulary.DataModels;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    public class WordDocumentationGenerator
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

                Table table = new Table();

                List<TableCell> headerRow = new List<TableCell>();

                TableCell titleTableCell = new TableCell();
                titleTableCell.Content.Add("title");

                headerRow.Add(titleTableCell);


                TableCell idTableCell = new TableCell();
                idTableCell.Content.Add("id");

                headerRow.Add(idTableCell);

                TableCell revisionTableCell = new TableCell();
                revisionTableCell.Content.Add("revision");

                headerRow.Add(revisionTableCell);

                TableCell typeTableCell = new TableCell();
                typeTableCell.Content.Add("type");

                headerRow.Add(typeTableCell);

                TableCell descriptionTableCell = new TableCell();
                descriptionTableCell.Content.Add("description");

                headerRow.Add(descriptionTableCell);

                table.TableCells.Add(headerRow);

                foreach (DataType dataType in domainClasses.DataTypes)
                {
                    List<TableCell> contentRow = new List<TableCell>()
                    {
                        new TableCell(dataType.Title),
                        new TableCell(dataType.ID),
                        new TableCell(dataType.Revision),
                        new TableCell(dataType.Type)
                    };

                    contentRow.Add(GetDataTypeDescription(dataType));

                    table.TableCells.Add(contentRow);
                }

                result += table.GenerateGridTable();
            }

            // properties
            if (domainClasses.PropertyClasses != null && domainClasses.PropertyClasses.Count != 0)
            {
                result += "### Property classes of domain " + domainName + Environment.NewLine;

                Table table = new Table();

                List<TableCell> headerRow = new List<TableCell>();

                TableCell titleTableCell = new TableCell();
                titleTableCell.Content.Add("title");

                headerRow.Add(titleTableCell);


                TableCell idTableCell = new TableCell();
                idTableCell.Content.Add("id");

                headerRow.Add(idTableCell);

                TableCell revisionTableCell = new TableCell();
                revisionTableCell.Content.Add("revision");

                headerRow.Add(revisionTableCell);

                TableCell typeTableCell = new TableCell();
                typeTableCell.Content.Add("dataType");

                headerRow.Add(typeTableCell);

                TableCell descriptionTableCell = new TableCell();
                descriptionTableCell.Content.Add("description");

                headerRow.Add(descriptionTableCell);

                table.TableCells.Add(headerRow);

                foreach (PropertyClass propertyClass in domainClasses.PropertyClasses)
                {
                    List<TableCell> contentRow = new List<TableCell>()
                    {
                        new TableCell(propertyClass.Title),
                        new TableCell(propertyClass.ID),
                        new TableCell(propertyClass.Revision),
                        new TableCell(propertyClass.GetDataTypeTitle(_specIfMetadataReader)),
                        new TableCell(propertyClass.Description[0].Text)
                    };

                    table.TableCells.Add(contentRow);
                }

                result += table.GenerateGridTable();

            }

            // resources
            if (domainClasses.ResourceClasses != null && domainClasses.ResourceClasses.Count != 0)
            {
                result += "### Resource classes of domain " + domainName + Environment.NewLine;

                Table table = new Table();

                List<TableCell> headerRow = new List<TableCell>();

                TableCell titleTableCell = new TableCell();
                titleTableCell.Content.Add("title");

                headerRow.Add(titleTableCell);


                TableCell idTableCell = new TableCell();
                idTableCell.Content.Add("id");

                headerRow.Add(idTableCell);

                TableCell revisionTableCell = new TableCell();
                revisionTableCell.Content.Add("revision");

                headerRow.Add(revisionTableCell);

                TableCell descriptionTableCell = new TableCell();
                descriptionTableCell.Content.Add("description");

                headerRow.Add(descriptionTableCell);

                table.TableCells.Add(headerRow);

                foreach (ResourceClass resourceClass in domainClasses.ResourceClasses)
                {
                    List<TableCell> contentRow = new List<TableCell>()
                    {
                        new TableCell(resourceClass.Title),
                        new TableCell(resourceClass.ID),
                        new TableCell(resourceClass.Revision)
                    };

                    contentRow.Add(GetResourceClassDescription(resourceClass));

                    table.TableCells.Add(contentRow);

                }

                result += table.GenerateGridTable();

            }

            // statements
            if (domainClasses.StatementClasses != null && domainClasses.StatementClasses.Count != 0)
            {
                result += "### Statement classes of domain " + domainName + Environment.NewLine;

                Table table = new Table();

                List<TableCell> headerRow = new List<TableCell>();

                TableCell titleTableCell = new TableCell();
                titleTableCell.Content.Add("title");

                headerRow.Add(titleTableCell);


                TableCell idTableCell = new TableCell();
                idTableCell.Content.Add("id");

                headerRow.Add(idTableCell);

                TableCell revisionTableCell = new TableCell();
                revisionTableCell.Content.Add("revision");

                headerRow.Add(revisionTableCell);

                TableCell descriptionTableCell = new TableCell();
                descriptionTableCell.Content.Add("description");

                headerRow.Add(descriptionTableCell);

                table.TableCells.Add(headerRow);

                foreach (StatementClass statementClass in domainClasses.StatementClasses)
                {
                    List<TableCell> contentRow = new List<TableCell>()
                    {
                        new TableCell(statementClass.Title),
                        new TableCell(statementClass.ID),
                        new TableCell(statementClass.Revision),
                    };

                    contentRow.Add(GetResourceClassDescription(statementClass));

                    table.TableCells.Add(contentRow);

                }

                result += table.GenerateGridTable();
            }

            return result;
        }

        private TableCell GetDataTypeDescription(DataType dataType)
        {
            TableCell result = new TableCell();

            if (dataType.Type == "xs:enumeration")
            {
                result.Content.Add(dataType.Description[0].Text);
                
                if (dataType.Values != null)
                {
                    foreach (EnumerationValue value in dataType.Values)
                    {
                        result.Content.Add("<p>" + value.Value[0].Text + " [" + value.ID + "]</p>");
                    }
                }
            }
            else
            {
                if (dataType.Description.ToString() == "[]")
                {
                    result.Content.Add("");
                }
                else
                {
                    if (dataType.Description.Count > 0)
                    {
                        result.Content.Add(dataType.Description[0].Text);
                    }
                }
            }

            return result;
        }

        private TableCell GetResourceClassDescription(ResourceClass resourceClass)
        {
            TableCell result = new TableCell();

            result.Content.Add(resourceClass.Description[0].Text);

            if(resourceClass.PropertyClasses != null && resourceClass.PropertyClasses.Count != 0)
            {
                result.Content.Add("<p>Property classes:</p>");

                foreach(Key key in resourceClass.PropertyClasses)
                {
                    PropertyClass propertyClass = _specIfMetadataReader.GetPropertyClassByKey(key);

                    result.Content.Add("<p>" + key.GetPropertyClassTitle(_specIfMetadataReader) + " [" + propertyClass.ID + " "+ propertyClass.Revision + "]</p>");
                }
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