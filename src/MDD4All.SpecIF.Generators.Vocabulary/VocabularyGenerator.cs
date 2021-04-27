using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.BaseTypes;
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    public class VocabularyGenerator
    {

        private Dictionary<string, SpecIF.DataModels.SpecIF> _domainClasses = new Dictionary<string, SpecIF.DataModels.SpecIF>();

        private SpecIF.DataModels.SpecIF _vocabulary;

        public SpecIF.DataModels.SpecIF GenerateVocabulary(string[] classDefinitionRoot)
        {
            _vocabulary = new SpecIF.DataModels.SpecIF()
            {
                CreatedAt = DateTime.Now,
                Generator = "SpecIFicator Vocabulary Generator",
                Title = new List<MultilanguageText> {
                    new MultilanguageText("SpecIF Vocabulary")
                }
            };

            // read all datatype and class definition
            foreach (string path in classDefinitionRoot)
            {
                DirectoryInfo classDefinitionRootDirectory = new DirectoryInfo(path);

                foreach(DirectoryInfo domainDirectoryInfo in classDefinitionRootDirectory.GetDirectories())
                {
                    InitializeClassDefinitions(domainDirectoryInfo);
                }

            }

            Resource vocabularyHeading = GenerateHeading("SpecIF Vocabulary");

            _vocabulary.Resources.Add(vocabularyHeading);

            Node rootNode = new Node()
            {
                ChangedAt = DateTime.Now,
                Title = new List<MultilanguageText> {
                    new MultilanguageText("SpecIF Vocabulary")
                },
                ResourceReference = new Key(vocabularyHeading.ID, vocabularyHeading.Revision)
            };

            _vocabulary.Hierarchies.Add(rootNode);

            foreach (KeyValuePair<string, SpecIF.DataModels.SpecIF> domain in _domainClasses)
            {
                GenerateDomainDescription(domain.Key, domain.Value, rootNode);
            }

            return _vocabulary;
        }

        private void GenerateDomainDescription(string domainName, SpecIF.DataModels.SpecIF domainClasses, Node parentNode)
        {
            Resource domainHeading = GenerateHeading("Domain " + domainName);

            _vocabulary.Resources.Add(domainHeading);

            Node domainHeadingNode = new Node()
            {
                ChangedAt = DateTime.Now,
                Title = new List<MultilanguageText> {
                    new MultilanguageText("Doamin " + domainName)
                },
                ResourceReference = new Key(domainHeading.ID, domainHeading.Revision)
            };

            parentNode.Nodes.Add(domainHeadingNode);

            // properties
            if (domainClasses.PropertyClasses != null && domainClasses.PropertyClasses.Count != 0)
            {
                Resource propertyHeading = GenerateHeading("Properties");

                _vocabulary.Resources.Add(propertyHeading);

                Node propertyHeadingNode = new Node()
                {
                    ChangedAt = DateTime.Now,
                    Title = new List<MultilanguageText> {
                        new MultilanguageText("Properties")
                    },
                    ResourceReference = new Key(propertyHeading.ID, propertyHeading.Revision)
                };

                domainHeadingNode.Nodes.Add(propertyHeadingNode);

                foreach (PropertyClass propertyClass in domainClasses.PropertyClasses)
                {
                    Resource propertyDescription = GeneratePropertyDescription(propertyClass);

                    _vocabulary.Resources.Add(propertyDescription);

                    Node node = new Node()
                    {
                        ChangedAt = DateTime.Now,

                        ResourceReference = new Key(propertyDescription.ID, propertyDescription.Revision)
                    };

                    propertyHeadingNode.Nodes.Add(node);
                }
            }

            // resources
            if (domainClasses.ResourceClasses != null && domainClasses.ResourceClasses.Count != 0)
            {
                
                Resource resourceHeading = GenerateHeading("Resources");

                _vocabulary.Resources.Add(resourceHeading);

                Node resourceHeadingNode = new Node()
                {
                    ChangedAt = DateTime.Now,
                    Title = new List<MultilanguageText> {
                        new MultilanguageText("Resources")
                    },
                    ResourceReference = new Key(resourceHeading.ID, resourceHeading.Revision)
                };

                domainHeadingNode.Nodes.Add(resourceHeadingNode);

                foreach (ResourceClass resourceClass in domainClasses.ResourceClasses)
                {
                    Resource resourceDescription = GenerateResourceDescription(resourceClass);

                    _vocabulary.Resources.Add(resourceDescription);

                    Node node = new Node()
                    {
                        ChangedAt = DateTime.Now,

                        ResourceReference = new Key(resourceDescription.ID, resourceDescription.Revision)
                    };

                    resourceHeadingNode.Nodes.Add(node);
                }
            }

            // statements
            if (domainClasses.StatementClasses != null && domainClasses.StatementClasses.Count != 0)
            {

                Resource statementHeading = GenerateHeading("Statements");

                _vocabulary.Resources.Add(statementHeading);

                Node statementHeadingNode = new Node()
                {
                    ChangedAt = DateTime.Now,
                    Title = new List<MultilanguageText> {
                        new MultilanguageText("Statements")
                    },
                    ResourceReference = new Key(statementHeading.ID)
                };

                domainHeadingNode.Nodes.Add(statementHeadingNode);

                foreach (StatementClass resourceClass in domainClasses.StatementClasses)
                {
                    Resource statementDescription = GenerateStatementDescription(resourceClass);

                    _vocabulary.Resources.Add(statementDescription);

                    Node node = new Node()
                    {
                        ChangedAt = DateTime.Now,

                        ResourceReference = new Key(statementDescription.ID, statementDescription.Revision)
                    };

                    statementHeadingNode.Nodes.Add(node);
                }
            }
        }

        private Resource GenerateHeading(string title)
        {
            Resource result = new Resource()
            {
                ChangedAt = DateTime.Now,
                Class = new Key("RC-Folder", "1"),
                ID = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                Properties = new List<Property>()
            };

            Property nameProperty = new Property(new Key("PC-Name", "1.1"), title);

            result.Properties.Add(nameProperty);


            return result;
        }

        private Resource GeneratePropertyDescription(PropertyClass propertyClass)
        {
            Resource result = new Resource()
            {
                ChangedAt = DateTime.Now,
                Class = new Key("RC-PropertyTerm", "1"),
                ID = "RS-" + propertyClass.ID,
                Properties = new List<Property>()
            };

            Property nameProperty = new Property(new Key("PC-Name", "1.1"), Value.ToSimpleTextString(propertyClass.Title) + " [Property Class]");

            result.Properties.Add(nameProperty);

            string description = GetDescription(Value.ToSimpleTextString(propertyClass.Title),
                                                        Value.ToSimpleTextString(propertyClass.Description),
                                                        propertyClass);

            description += "<dt>Data Type</dt>";

            description += "<dd>" + propertyClass.DataType + "</dd>";

            Property descriptionProperty = new Property(new Key("PC-Description", "1"),description);

            result.Properties.Add(descriptionProperty);

            return result;
        }

        private Resource GenerateResourceDescription(ResourceClass resourceClass)
        {
            Resource result = new Resource()
            {
                ChangedAt = DateTime.Now,
                Class = new Key("RC-ResourceTerm", "1"),
                ID = "RS-" + resourceClass.ID,
                Properties = new List<Property>()
            };

            Property nameProperty = new Property(new Key("PC-Name", "1"), Value.ToSimpleTextString(resourceClass.Title) + " [Resource Class]");

            result.Properties.Add(nameProperty);

            string resourceDescription = GetDescription(Value.ToSimpleTextString(resourceClass.Title),
                                                        Value.ToSimpleTextString(resourceClass.Description),
                                                        resourceClass);

            if (resourceClass.PropertyClasses != null && resourceClass.PropertyClasses.Count != 0)
            {
                resourceDescription += "<dt>Property Classes</dt>";

                foreach (Key propertyReference in resourceClass.PropertyClasses)
                {
                    resourceDescription += "<dd>" + propertyReference.ID + " (" + propertyReference.Revision + ")</dd>";
                }
            }

            Property descriptionProperty = new Property(new Key("PC-Description", "1"), resourceDescription);

            result.Properties.Add(descriptionProperty);

            return result;
        }

        private Resource GenerateStatementDescription(StatementClass statementClass)
        {
            Resource result = new Resource()
            {
                ChangedAt = DateTime.Now,
                Class = new Key("RC-ResourceTerm", "1"),
                ID = "RS-" + statementClass.ID,
                Properties = new List<Property>()
            };

            Property nameProperty = new Property(new Key("PC-Name", "1"), Value.ToSimpleTextString(statementClass.Title) + " [Statement Class]");

            result.Properties.Add(nameProperty);

            string resourceDescription = GetDescription(Value.ToSimpleTextString(statementClass.Title),
                                                        Value.ToSimpleTextString(statementClass.Description),
                                                        statementClass);

            if (statementClass.PropertyClasses != null && statementClass.PropertyClasses.Count != 0)
            {
                resourceDescription += "<dt>Property Classes</dt>";

                foreach (Key propertyReference in statementClass.PropertyClasses)
                {
                    resourceDescription += "<dd>" + propertyReference.ID + " (" + propertyReference.Revision + ")</dd>";
                }
            }

            Property descriptionProperty = new Property(new Key("PC-Description", "1"), resourceDescription);

            result.Properties.Add(descriptionProperty);

            return result;
        }

        private string GetDescription(string title, string description, IdentifiableElement element)
        {
            string result = "<dl><dt>Title</dt><dd>" + title + "</dd>";

            result += "<dt>Description</dt><dd>" + description + "</dd></dt>";

            result += "<dt>ID</dt><dd>" + element.ID + "</dd></dt>";

            result += "<dt>Revision</dt><dd>" + element.Revision + "</dd></dt>";

            return result;
        }

        private void InitializeClassDefinitions(DirectoryInfo domainDirectory)
        {
            string domainName = domainDirectory.Name;

            FileInfo[] specIfFiles = domainDirectory.GetFiles("*.specif");

            SpecIF.DataModels.SpecIF domainSpecIF = null;

            int fileConuter = 0;

            foreach(FileInfo fileInfo in specIfFiles)
            {
                fileConuter++;

                SpecIF.DataModels.SpecIF specIF = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(fileInfo.FullName);

                if(fileConuter == 1)
                {
                    domainSpecIF = specIF;
                }
                else
                {
                    domainSpecIF.DataTypes.AddRange(specIF.DataTypes);
                    domainSpecIF.PropertyClasses.AddRange(specIF.PropertyClasses);
                    domainSpecIF.ResourceClasses.AddRange(specIF.ResourceClasses);
                    domainSpecIF.StatementClasses.AddRange(specIF.StatementClasses);
                }
            }

            if(domainSpecIF != null)
            {
                _domainClasses.Add(domainName, domainSpecIF);
            }

            
        }

    }
}
