/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;
using MDD4All.EnterpriseArchitect.Manipulations;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    public class EaToSpecIfConverter
    {
        private EAAPI.Repository _repository;

        public EaToSpecIfConverter(EAAPI.Repository repository)
        {
            _repository = repository;
        }

        public Resource ConvertElementToResource(EAAPI.Element eaElement)
        {
            Resource result;

            result = new Resource();

            if (eaElement != null)
            {
                result.ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);

                result.Revision = Key.FIRST_MAIN_REVISION;

                result.ChangedAt = eaElement.Modified;

                result.ChangedBy = eaElement.Author;

                result.Class = new Key("RC-Requirement", 1);

                result.Properties = new List<Property>();

                string name = eaElement.Name;

                string classifierName = eaElement.GetClassifierName(_repository);

                if (!string.IsNullOrEmpty(classifierName))
                {
                    if (name == "")
                    {
                        name = classifierName;
                    }
                    else
                    {
                        name += " : " + classifierName;
                    }
                }

                result.Title = new Value(name);

                Property nameProperty = new Property
                {
                    Title = new Value("dcterms:title"),
                    PropertyClass = new Key("AT-Req-Name", 1),
                    Value = new Value
                    {
                        LanguageValues = new List<LanguageValue>
                        {
                            new LanguageValue
                            {
                                Text = name
                            }
                        }
                    },
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NAME"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author
                };

                result.Properties.Add(nameProperty);

                Property notesProperty = new Property
                {
                    Title = new Value("dcterms:description"),
                    PropertyClass = new Key("PC-Text", 1),
                    Value = new Value
                    {
                        LanguageValues = new List<LanguageValue>
                        {
                            new LanguageValue
                            {
                                Text = eaElement.Notes
                            }
                        }
                    },
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NOTES"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author
                };



                result.Properties.Add(notesProperty);


                string stereotype = eaElement.Stereotype;

                if (eaElement.Type == "Requirement")
                {
                    result.Class = new Key("RC-Requirement", 1);

                    string identifier = eaElement.GetTaggedValueString("Identifier");

                    if (!string.IsNullOrEmpty(identifier))
                    {
                        Property identifierProperty = new Property
                        {
                            Title = new Value("dcterms:identifier"),
                            PropertyClass = new Key("PC-VisibleId", 1),
                            Value = new Value
                            {
                                LanguageValues = new List<LanguageValue>
                                {
                                    new LanguageValue
                                    {
                                        Text = identifier
                                    }
                                }
                            },
                            ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_IDENTIFIER"),
                            ChangedAt = eaElement.Modified,
                            ChangedBy = eaElement.Author
                        };

                        result.Properties.Add(identifierProperty);
                    }

                    string status = eaElement.Status;

                    string statusID = "";

                    switch (status)
                    {
                        case "00_rejected":
                            statusID = "V-Status-1";
                            break;

                        case "10_initial":
                            statusID = "V-Status-2";
                            break;

                        case "20_drafted":
                            statusID = "V-Status-3";
                            break;

                        case "30_submitted":
                            statusID = "V-Status-4";
                            break;

                        case "40_approved":
                            statusID = "V-Req-Status-3";
                            break;
                    }

                    if (!string.IsNullOrEmpty(statusID))
                    {
                        Property statusProperty = new Property
                        {
                            Title = new Value("SpecIF:Status"),
                            PropertyClass = new Key("PC-Status", 1),
                            Value = new Value
                            {
                                LanguageValues = new List<LanguageValue>
                                    {
                                        new LanguageValue
                                        {
                                            Text = statusID
                                        }
                                    }
                            },
                            ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_STATUS"),
                            ChangedAt = eaElement.Modified,
                            ChangedBy = eaElement.Author
                        };

                        result.Properties.Add(statusProperty);
                    }


                }
                else if (eaElement.Type == "Package")
                {
                    result.Class = new Key("RC-Folder", 1);
                }
                else if (eaElement.Type == "Object")
                {
                    if (stereotype == "agent" || stereotype == "human agent")
                    {
                        result.Class = new Key("RC-Actor", 1);
                    }
                    else if (stereotype == "storage")
                    {
                        result.Class = new Key("RC-State", 1);
                    }
                    else if (stereotype == "heading")
                    {
                        result.Class = new Key("RC-Folder", 1);
                    }

                }
                else if (eaElement.Type == "Actor")
                {
                    result.Class = new Key("RC-Actor", 1);
                }
                else if (eaElement.Type == "Port")
                {
                    if (stereotype == "channel")
                    {
                        //result.ResourceClass = "OT-Channel";
                    }
                }
                

                
            }

            return result;
        }

        public Node ConvertPackageToHierarchy(EAAPI.Package eaPackage)
        {
            Node result = new Node();

			Resource resource = new Resource();

			resource.ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaPackage.Element.ElementGUID);
			resource.Title = new Value(eaPackage.Name);
			resource.Class = new Key("RC-Folder", 1);
			resource.Properties = new List<Property>();

            result.Nodes = new List<Node>();

            for(short count=0; count < eaPackage.Packages.Count; count++)
            {
                EAAPI.Package childPackage = eaPackage.Packages.GetAt(count) as EAAPI.Package;

                Node childNode = new Node()
                {
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID + "-NODE"),
                    Revision = Key.FIRST_MAIN_REVISION,
                    NodeReferences = new List<Key>(),
                    ResourceReference = new Key()
                    {
                        ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID),
                        Revision = Key.FIRST_MAIN_REVISION
                    }
               
                };

                ConvertPackageToHierarchyRecursively(childPackage, childNode);

                result.Nodes = new List<Node>();
                result.Nodes.Add(childNode);
               
            }

            for(short count = 0; count < eaPackage.Elements.Count; count++)
            {
                EAAPI.Element element = eaPackage.Elements.GetAt(count) as EAAPI.Element;

                ConvertElementToNodeRecursively(element, result.Nodes);
            }

            return result;
        }

        private void ConvertPackageToHierarchyRecursively(EAAPI.Package eaPackage, Node node)
        {
            for (short count = 0; count < eaPackage.Packages.Count; count++)
            {
                EAAPI.Package childPackage = eaPackage.Packages.GetAt(count) as EAAPI.Package;

                Node childNode = new Node()
                {
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID + "-NODE"),
                    Revision = Key.FIRST_MAIN_REVISION,
                    NodeReferences = new List<Key>(),
                    ResourceReference = new Key()
                    {
                        ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID),
                        Revision = Key.FIRST_MAIN_REVISION
					}

                };

                node.Nodes = new List<Node>();
                node.Nodes.Add(childNode);

                ConvertPackageToHierarchyRecursively(childPackage, childNode);
            }

            // diagrams
            //for (short count = 0; count < eaPackage.Diagrams.Count; count++)
            //{

            //}

            // elements
            for (short count = 0; count < eaPackage.Elements.Count; count++)
            {
                EAAPI.Element element = eaPackage.Elements.GetAt(count) as EAAPI.Element;

                ConvertElementToNodeRecursively(element, node.Nodes);

                
            }


        }

        private void ConvertElementToNodeRecursively(EAAPI.Element element, List<Node> parentNodeList)
        {
            Node elementNode = new Node()
            {
                ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID + "-NODE"),
                Revision = Key.FIRST_MAIN_REVISION,
                NodeReferences = new List<Key>(),
                ResourceReference = new Key()
                {
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID),
                    Revision = Key.FIRST_MAIN_REVISION
				}
            };

            parentNodeList.Add(elementNode);

            elementNode.Nodes = new List<Node>();

            for(short count = 0; count < element.Elements.Count; count++)
            {
                EAAPI.Element childElement = element.Elements.GetAt(count) as EAAPI.Element;
                
                ConvertElementToNodeRecursively(childElement, elementNode.Nodes);

            }


            for (short embeddedElementsCount = 0; embeddedElementsCount < element.EmbeddedElements.Count; embeddedElementsCount++)
            {
                EAAPI.Element embeddedElement = element.EmbeddedElements.GetAt(embeddedElementsCount) as EAAPI.Element;

                Node embeddedElementNode = new Node()
                {
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID + "-NODE"),
                    Revision = Key.FIRST_MAIN_REVISION,
                    NodeReferences = new List<Key>(),
                    ResourceReference = new Key()
                    {
                        ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID),
                        Revision = Key.FIRST_MAIN_REVISION
					}
                };

                if (elementNode.Nodes == null)
                {
                    elementNode.Nodes = new List<Node>();
                }

                elementNode.Nodes.Add(embeddedElementNode);
            }
        }
    }
}
