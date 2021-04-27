/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using EAAPI = EA;
using MDD4All.SpecIF.DataModels;
using MDD4All.EnterpriseArchitect.Manipulations;
using System.IO;
using System.Drawing;
using MDD4All.SpecIF.DataModels.Helpers;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.EnterpriseArchitect.SvgGenerator;
using MDD4All.SVG.DataModels;
using System.Xml.Serialization;
using System.Xml;
using MDD4All.SpecIF.DataModels.DiagramInterchange;
using MDD4All.SpecIF.DataModels.DiagramInterchange.BaseElements;
using Newtonsoft.Json;
using MDD4All.SpecIF.DataProvider.EA.Converters.DataModels;
using MDD4All.EAFacade.DataAccess.Cached;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    // TODO: attribute stereotypes, run states, behavior references

    public class EaUmlToSpecIfConverter
    {
        // IDs for primitive types
        private const string INTEGER_GUID = "{239A1D1D-046D-4902-87B1-4D86FB6B7D76}";
        private const string BOOLEAN_GUID = "{26844644-6F52-40A8-BDA8-43A61276B197}";
        private const string STRING_GUID = "{4CEEB645-62E8-44B0-A97E-694FF01C25E4}";
        private const string UNLIMITED_NATURAL_GUID = "{C35A5389-2CBF-47FB-B987-A540E71B0BFC}";
        private const string REAL_GUID = "{ABA6441D-268A-457D-A775-484C7301AF21}";

        private EAAPI.Repository _repository;

        //private Dictionary<string, Resource> _primitiveTypes = new Dictionary<string, Resource>();

        private Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
        private Dictionary<string, Statement> _statements = new Dictionary<string, Statement>();


        private Dictionary<string, ElementResource> _elementResources = new Dictionary<string, ElementResource>();
        private Dictionary<string, ImplicitElementResource> _implicitElementResources = new Dictionary<string, ImplicitElementResource>();
        private Dictionary<string, PortStateResource> _portStateResources = new Dictionary<string, PortStateResource>();
        private Dictionary<string, DiagramResource> _diagramResources = new Dictionary<string, DiagramResource>();

        private ISpecIfMetadataReader _metadataReader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository">The EA repository.</param>
        public EaUmlToSpecIfConverter(EAAPI.Repository repository, ISpecIfMetadataReader metadataReader)
        {
            _repository = repository;
            _metadataReader = metadataReader;

            InitializePrimitiveTypes();
        }

        private void ResetCache()
        {
            _resources = new Dictionary<string, Resource>();
            _statements = new Dictionary<string, Statement>();
            InitializePrimitiveTypes();
        }

        /// <summary>
        /// Convert a EA model to SpecIF. The conversion is done recusively starting from the given package
        /// in the project browser.
        /// </summary>
        /// <param name="selectedPackage">The start package for the conversion.</param>
        /// <returns>The generated SpecIF object.</returns>
        public MDD4All.SpecIF.DataModels.SpecIF ConvertModelToSpecIF(EAAPI.Package selectedPackage)
        {
            SpecIF.DataModels.SpecIF result = new SpecIF.DataModels.SpecIF();

            result.Title = new List<MultilanguageText>
            {
                new MultilanguageText("UML data extracted from Sparx Enterprise Architect: " + selectedPackage.Name)
            };
            result.Generator = "SpecIFicator";

            Node node = new Node();

            GetModelHierarchyRecursively(selectedPackage, node);

            result.Hierarchies.Add(node);

            foreach (KeyValuePair<string, Resource> keyValuePair in _resources)
            {
                result.Resources.Add(keyValuePair.Value);
            }

            foreach (KeyValuePair<string, Statement> keyValuePair in _statements)
            {
                result.Statements.Add(keyValuePair.Value);
            }


            return result;
        }

        public Resource GetResourceByKey(Key key)
        {
            Resource result = null;

            string cacheKey = GetCacheKey(key);

            if (_implicitElementResources.ContainsKey(cacheKey))
            {
                result = _implicitElementResources[cacheKey].Resource;
            }
            else if (_portStateResources.ContainsKey(cacheKey))
            {
                result = _portStateResources[cacheKey].Resource;

            }
            else if (_diagramResources.ContainsKey(cacheKey))
            {
                result = _diagramResources[cacheKey].Resource;
            }

            if (result == null)
            {
                string eaGUID = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(key.ID);

                EAAPI.Element element = _repository.GetElementByGuid(eaGUID);

                if (element != null)
                {
                    result = ConvertElement(element);
                }
                else
                {
                    // try diagram
                    EAAPI.Diagram diagram = _repository.GetDiagramByGuid(eaGUID) as EAAPI.Diagram;

                    if (diagram != null)
                    {
                        result = ConvertDiagram(diagram);
                    }


                }



            }

            if (result == null)
            {
                ;
            }

            return result;
        }

        public Statement GetStatementByID(string id)
        {
            Statement result = null;

            if (_statements.ContainsKey(id))
            {
                result = _statements[id];
            }

            return result;
        }

        public List<Statement> GetAllStatementsForResource(Key resourceKey)
        {
            List<Statement> result = new List<Statement>();


            // implicit: contains statements, shows statements, classifier statements

            string cacheKey = GetCacheKey(resourceKey);

            if (_elementResources.ContainsKey(cacheKey))
            {
                ElementResource resource = _elementResources[cacheKey];

                // implicit statements
                result.AddRange(resource.ImplicitElementsStatements);

                // explicit statements
                EAAPI.Element element = _repository.GetElementByGuid(resource.EaGUID);

                if (element != null)
                {
                    result.AddRange(ConvertExplicitElementStatements(element));
                }
                else
                {
                    // try diagram
                    EAAPI.Diagram diagram = _repository.GetDiagramByGuid(resource.EaGUID) as EAAPI.Diagram;

                    if (diagram != null)
                    {
                        result.AddRange(ConvertExplicitDiagramStatements(diagram));
                    }
                }
            }
            else if (_implicitElementResources.ContainsKey(cacheKey))
            {
                ImplicitElementResource resource = _implicitElementResources[cacheKey];

                result.AddRange(resource.ImplicitElementsStatements);
            }
            else if (_portStateResources.ContainsKey(cacheKey))
            {
                PortStateResource portStateResource = _portStateResources[cacheKey];

                foreach (KeyValuePair<string, Statement> portStateStatement in portStateResource.Statements)
                {
                    result.Add(portStateStatement.Value);
                }
            }
            else if (_diagramResources.ContainsKey(cacheKey))
            {
                DiagramResource diagramResource = _diagramResources[cacheKey];

                result.AddRange(diagramResource.ImplicitDiagramStatements);
            }
            else
            {
                ;
            }

            return result;
        }

        private Node _hierarchy = null;

        public Node GetHierarchy()
        {
            Node result = null;

            Node node = new Node();

            //ResetCache();

            //if (_hierarchy == null)
            //{
            EAAPI.Package package = _repository.GetPackageByGuid("{45A143E0-D43A-4f51-ACA6-FF695EEE3256}");
            // 
            //EAAPI.Package package = _repository.GetPackageByGuid("{962FD96B-4E7C-43cf-89B8-4D29D19C0A0F}");

            GetModelHierarchyRecursively(package, node);

            //_hierarchy = node;
            result = node;

            string json = JsonConvert.SerializeObject(node);

            System.IO.File.WriteAllText(@"d:\test\specif\hierarchy.json", json);

            //}
            //else
            //         {
            //	result = _hierarchy;
            //         }
            return result;
        }

        private void GetModelHierarchyRecursively(EAAPI.Package currentPackage, Node currentNode)
        {
            Console.WriteLine("Package: " + currentPackage.Name);


            string packageID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentPackage.Element.ElementGUID);
            string packageRevision = EaDateToRevisionConverter.ConvertDateToRevision(currentPackage.Element.Modified);

            Key pakageKey = new Key(packageID, packageRevision);

            currentNode.ResourceReference = pakageKey;
            currentNode.Description = new List<MultilanguageText> {
                new MultilanguageText("Package: " + currentPackage.Name)
            };

            // diagrams
            for (short diagramCounter = 0; diagramCounter < currentPackage.Diagrams.Count; diagramCounter++)
            {
                EAAPI.Diagram diagram = currentPackage.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

                string diagramID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
                string diagramRevision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

                Key diagramKey = new Key(diagramID, diagramRevision);

                Node node = new Node()
                {
                    ResourceReference = diagramKey,
                    Description = new List<MultilanguageText> {
                        new MultilanguageText("Diagram: " + diagram.Name)
                    }
                };

                currentNode.Nodes.Add(node);
            }

            // recursive call for child packages
            for (short packageCounter = 0; packageCounter < currentPackage.Packages.Count; packageCounter++)
            {
                EAAPI.Package childPackage = currentPackage.Packages.GetAt(packageCounter) as EAAPI.Package;

                //Console.WriteLine("Recursive call: " + childPackage.Name);

                Node childNode = new Node()
                {
                };
                currentNode.Nodes.Add(childNode);

                GetModelHierarchyRecursively(childPackage, childNode);

            }

            // elements
            for (short elementCounter = 0; elementCounter < currentPackage.Elements.Count; elementCounter++)
            {
                EAAPI.Element element = currentPackage.Elements.GetAt(elementCounter) as EAAPI.Element;

                GetElementHierarchyRecursively(element, currentNode);
            }
        }

        private void GetElementHierarchyRecursively(EAAPI.Element currentElement, Node currentNode)
        {
            Console.WriteLine("Element: " + currentElement.Name);

            string elementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentElement.ElementGUID);
            string elementRevision = EaDateToRevisionConverter.ConvertDateToRevision(currentElement.Modified);

            Key elememntKey = new Key(elementID, elementRevision);

            Node node = new Node()
            {
                ResourceReference = elememntKey,
                Description = new List<MultilanguageText> {
                    new MultilanguageText("Element: " + currentElement.Name)
                }
            };

            currentNode.Nodes.Add(node);

            // diagrams
            for (short diagramCounter = 0; diagramCounter < currentElement.Diagrams.Count; diagramCounter++)
            {
                EAAPI.Diagram diagram = currentElement.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

                string diagramID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
                string diagramRevision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

                Key diagramKey = new Key(diagramID, diagramRevision);

                Node diagramNode = new Node()
                {
                    ResourceReference = diagramKey,
                    Description = new List<MultilanguageText> {
                        new MultilanguageText("Diagram: " + diagram.Name)
                    }
                };

                node.Nodes.Add(diagramNode);
            }

            // sub-elements
            for (short counter = 0; counter < currentElement.Elements.Count; counter++)
            {
                EAAPI.Element subElement = currentElement.Elements.GetAt(counter) as EAAPI.Element;

                string subElementType = subElement.Type;

                if (subElementType != "Port" && subElementType != "ActionPin" && subElementType != "ActivityParameter")
                {
                    // recursive call
                    GetElementHierarchyRecursively(subElement, node);
                }

            }

            // embedded elements
            for (short embeddedElementCounter = 0; embeddedElementCounter < currentElement.EmbeddedElements.Count; embeddedElementCounter++)
            {
                EAAPI.Element embeddedElement = currentElement.EmbeddedElements.GetAt(embeddedElementCounter) as EAAPI.Element;

                Console.WriteLine("Embedded element: " + embeddedElement.Name);

                string embeddedElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID);
                string embeddedElementRevision = EaDateToRevisionConverter.ConvertDateToRevision(embeddedElement.Modified);

                Node embeddedElementNode = new Node()
                {
                    ResourceReference = new Key(embeddedElementID, embeddedElementRevision),
                    Description = new List<MultilanguageText> {
                        new MultilanguageText("Embedded element: " + embeddedElement.Name)
                    }

                };

                node.Nodes.Add(embeddedElementNode);
            }


        }



        private string GetCacheKey(Resource resource)
        {
            string result = resource.ID + "_R_" + resource.Revision;

            return result;
        }

        private string GetCacheKey(string id, string revision)
        {
            string result = id + "_R_" + revision;

            return result;
        }

        private string GetCacheKeyForElement(EAAPI.Element element)
        {
            string result = "";

            result += EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID);

            result += "_R_" + EaDateToRevisionConverter.ConvertDateToRevision(element.Modified);

            return result;
        }

        private string GetCacheKeyForDiagram(EAAPI.Diagram diagram)
        {
            string result = "";

            result += EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

            result += "_R_" + EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

            return result;
        }

        private string GetCacheKey(Key key)
        {
            string result = key.ID + "_R_" + key.Revision;

            return result;
        }

        #region DATA_ADDITION







        //private void AddAttributes(EAAPI.Element eaElement, Resource elementResource)
        //{
        //	for(short counter = 0; counter < eaElement.Attributes.Count; counter++ )
        //	{
        //		EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

        //		if(attribute != null)
        //		{
        //			Resource attributeResource = ConvertAttribute(attribute, eaElement, elementResource.Revision);

        //			if (!_resources.ContainsKey(attributeResource.ID))
        //			{
        //				_resources.Add(attributeResource.ID, attributeResource);
        //			}

        //			// add contains statement
        //			Statement containsStatement = GetContainsStatement(elementResource, attributeResource);
        //			if (!_statements.ContainsKey(containsStatement.ID))
        //			{
        //				_statements.Add(containsStatement.ID, containsStatement);
        //			}

        //			if (attribute.ClassifierID == 0 && !string.IsNullOrEmpty(attribute.Type))
        //			{
        //				Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(attribute.Type);

        //				if (primitiveClassifier != null)
        //				{
        //					Statement classifierStatement = GetClassifierStatement(primitiveClassifier, attributeResource);

        //					if (!_statements.ContainsKey(classifierStatement.ID))
        //					{
        //						_statements.Add(classifierStatement.ID, classifierStatement);
        //					}
        //				}
        //			}
        //			else if (attribute.ClassifierID != 0)
        //			{
        //				EAAPI.Element classifierElement = attribute.GetClassifier(_repository);

        //				string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);

        //				Resource classifierResource = null;

        //				if (!_resources.ContainsKey(classifierSpecIfID))
        //				{
        //					classifierResource = AddElement(classifierElement);
        //				}
        //				else
        //				{
        //					classifierResource = _resources[classifierSpecIfID];
        //				}


        //				Statement classifierStatement = GetClassifierStatement(classifierResource, attributeResource);

        //				if (!_statements.ContainsKey(classifierStatement.ID))
        //				{
        //					_statements.Add(classifierStatement.ID, classifierStatement);
        //				}
        //			}

        //		}
        //	}

        //}

        //private void AddOperations(EAAPI.Element eaElement, Resource elementResource)
        //{
        //	for (short counter = 0; counter < eaElement.Methods.Count; counter++)
        //	{
        //		EAAPI.Method operation = eaElement.Methods.GetAt(counter) as EAAPI.Method;

        //		if (operation != null)
        //		{
        //			Resource operationResource = ConvertOperation(operation, eaElement, elementResource.Revision);

        //			if (!_resources.ContainsKey(operationResource.ID))
        //			{
        //				_resources.Add(operationResource.ID, operationResource);
        //			}

        //			Statement containsStatement = GetContainsStatement(elementResource, operationResource);

        //			if (!_statements.ContainsKey(containsStatement.ID))
        //			{
        //				_statements.Add(containsStatement.ID, containsStatement);
        //			}

        //			Console.WriteLine("Operation " + operation.Name + " classifierID=" + operation.ClassifierID);

        //			if (operation.ClassifierID == "0" && !string.IsNullOrEmpty(operation.ReturnType))
        //			{
        //				Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(operation.ReturnType);

        //				if (primitiveClassifier != null)
        //				{
        //					Statement classifierStatement = GetClassifierStatement(primitiveClassifier, operationResource);

        //					if (!_statements.ContainsKey(classifierStatement.ID))
        //					{
        //						_statements.Add(classifierStatement.ID, classifierStatement);
        //					}
        //				}
        //			}
        //			else if (operation.ClassifierID != "0")
        //			{
        //				int classifierID = int.Parse(operation.ClassifierID);

        //				Console.WriteLine("Operation complex classifier. ID=" + classifierID);

        //				EAAPI.Element classifierElement = operation.GetClassifier(_repository);

        //				string cacheKey = GetCacheKeyForElement(classifierElement);

        //				if (!_resources.ContainsKey(cacheKey))
        //				{
        //					Resource classifierResource = AddElement(classifierElement);

        //					Statement classifierStatement = GetClassifierStatement(classifierResource, operationResource);

        //					if (!_statements.ContainsKey(classifierStatement.ID))
        //					{
        //						_statements.Add(classifierStatement.ID, classifierStatement);
        //					}
        //				}
        //				else
        //                      {
        //					Resource classifierResource = _resources[cacheKey];

        //					Statement classifierStatement = GetClassifierStatement(classifierResource, operationResource);

        //					if (!_statements.ContainsKey(classifierStatement.ID))
        //					{
        //						_statements.Add(classifierStatement.ID, classifierStatement);
        //					}
        //				}
        //			}


        //			for (short parameterCounter = 0; parameterCounter < operation.Parameters.Count; parameterCounter++)
        //			{
        //				EAAPI.Parameter parameterEA = operation.Parameters.GetAt(parameterCounter) as EAAPI.Parameter;

        //				Resource operationParameterResource = ConvertOperationParameter(parameterEA, eaElement, elementResource.Revision);

        //				if (!_resources.ContainsKey(operationParameterResource.ID))
        //				{
        //					_resources.Add(operationParameterResource.ID, operationParameterResource);
        //				}

        //				Statement paramaterConatinmentStatement = GetContainsStatement(operationResource, operationParameterResource);

        //				if (!_statements.ContainsKey(paramaterConatinmentStatement.ID))
        //				{
        //					_statements.Add(paramaterConatinmentStatement.ID, paramaterConatinmentStatement);
        //				}

        //				if (parameterEA.ClassifierID == "0" && !string.IsNullOrEmpty(parameterEA.Type))
        //				{
        //					Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(parameterEA.Type);

        //					if (primitiveClassifier != null)
        //					{

        //						Statement classifierStatement = GetClassifierStatement(primitiveClassifier, operationParameterResource);

        //						if (!_statements.ContainsKey(classifierStatement.ID))
        //						{
        //							_statements.Add(classifierStatement.ID, classifierStatement);
        //						}
        //					}
        //				}
        //				else if (parameterEA.ClassifierID != "0")
        //				{
        //                          EAAPI.Element classifierElement = parameterEA.GetClassifier(_repository);

        //                          string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);

        //                          Resource classifierResource = null;

        //                          if (!_resources.ContainsKey(classifierSpecIfID))
        //                          {
        //                              classifierResource = AddElement(classifierElement);
        //                          }
        //                          else
        //                          {
        //                              classifierResource = _resources[classifierSpecIfID];
        //                          }

        //                          Statement classifierStatement = GetClassifierStatement(classifierResource, operationParameterResource);

        //					if (!_statements.ContainsKey(classifierStatement.ID))
        //					{
        //						_statements.Add(classifierStatement.ID, classifierStatement);
        //					}
        //				}

        //			}
        //		}
        //	}
        //}

        //private void AddElementConstraints(EAAPI.Element eaElement, Resource elementResource)
        //{
        //string connectorSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);

        //// connector constraints
        //for (short constraintCounter = 0; constraintCounter < eaElement.Constraints.Count; constraintCounter++)
        //{
        //	EAAPI.Constraint elementConstraintEA = eaElement.Constraints.GetAt(constraintCounter) as EAAPI.Constraint;

        //	if (elementConstraintEA != null)
        //	{
        //		Resource connectorConstraint = ConvertElementConstraint(eaElement, elementConstraintEA, constraintCounter + 1);

        //		if (!_resources.ContainsKey(connectorConstraint.ID))
        //		{
        //			_resources.Add(connectorConstraint.ID, connectorConstraint);
        //		}

        //		Statement constraintContainsStatement = GetContainsStatementFromSpecIfID(new Key(elementResource.ID, elementResource.Revision),
        //			new Key(connectorConstraint.ID));

        //		if (!_statements.ContainsKey(constraintContainsStatement.ID))
        //		{
        //			_statements.Add(constraintContainsStatement.ID, constraintContainsStatement);
        //		}


        //	}
        //}
        //}


        #endregion

        #region CONVERTERS

        public Resource ConvertElement(EAAPI.Element eaElement)
        {
            Resource result;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);
            string revision = EaDateToRevisionConverter.ConvertDateToRevision(eaElement.Modified);

            string cacheKey = GetCacheKeyForElement(eaElement);


            string elementType = eaElement.Type;

            EAAPI.Package elementPackage;

            if (elementType == "Package")
            {
                elementPackage = _repository.GetPackageByGuid(eaElement.ElementGUID);
            }
            else
            {
                elementPackage = _repository.GetPackageByID(eaElement.PackageID);
            }

            ResourceClass resourceClass = GetResourceClassForElementType(elementType, eaElement.Stereotype);

            if (!_elementResources.ContainsKey(cacheKey))
            {

                #region RESOURCE_CREATION


                Resource elementResource = new Resource()
                {
                    ID = specIfID,
                    Class = new Key(resourceClass.ID, resourceClass.Revision),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author,
                    Revision = revision
                };

                elementResource.Properties = new List<Property>();

                elementResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Name", "1.1"),
                        Value = eaElement.Name,

                    }
                    );

                elementResource.Properties.Add(
                    new Property()
                    {

                        Class = new Key("PC-Description", "1.1"),
                        Value = eaElement.Notes,

                    }
                    );

                if (resourceClass.ID != "RC-Hierarchy")
                {
                    elementResource.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Status", "1.1"),
                            Value = GetStatusEnumID(eaElement.Status),

                        }
                        );
                }

                if (resourceClass.ID == "RC-State" ||
                    resourceClass.ID == "RC-Actor" ||
                    resourceClass.ID == "RC-Collection" ||
                    resourceClass.ID == "RC-Event")
                {
                    string namespc = "";
                    string stereotype = "";

                    ParseFullQualifiedName(eaElement.FQStereotype, out namespc, out stereotype);

                    string stereotypeValue = "";

                    if (namespc != "")
                    {
                        stereotypeValue = namespc + ":" + stereotype;
                    }
                    else
                    {
                        stereotypeValue = stereotype;
                    }

                    elementResource.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Stereotype", "1.1"),
                            Value = stereotypeValue
                        }
                        );

                    string elementLegacyType = "";

                    if (stereotype == "")
                    {
                        switch (elementType)
                        {
                            case "StateNode":

                                if (eaElement.Subtype == 100)
                                {
                                    elementLegacyType = "OMG:UML:2.5.1:InitialNode";
                                }
                                else if (eaElement.Subtype == 101)
                                {
                                    elementLegacyType = "OMG:UML:2.5.1:ActivityFinal";
                                }
                                else if (eaElement.Subtype == 102)
                                {
                                    elementLegacyType = "OMG:UML:2.5.1:FlowFinal";
                                }

                                break;

                            case "Decision":
                                elementLegacyType = "OMG:UML:2.5.1:DecisionNode";
                                break;

                            default:
                                elementLegacyType = "OMG:UML:2.5.1:" + elementType;
                                break;
                        }

                    }
                    else
                    {
                        elementLegacyType = "OMG:UML:2.5.1:" + elementType;

                        //if (elementType == "Object" && stereotype == "agent")
                        //{
                        //	elementLegacyType = "FMC4SE:Agent";
                        //}
                        //if (elementType == "Port" && stereotype == "channel")
                        //{
                        //	elementLegacyType = "FMC4SE:Channel";
                        //}
                    }

                    elementResource.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Type", "1.1"),
                            Value = elementLegacyType
                        }
                        );

                    string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID) + "_VISIBILITY";

                    elementResource.Properties.Add(GetVisibilityProperty(eaElement.Visibility, id, revision, eaElement.Modified, eaElement.Author));


                }

                if (eaElement.Type == "Requirement")
                {
                    string identifierValue = eaElement.GetTaggedValueString("Identifier");

                    if (!string.IsNullOrEmpty(identifierValue))
                    {
                        elementResource.Properties.Add(
                            new Property()
                            {
                                Class = new Key("PC-VisibleId", "1.1"),
                                Value = identifierValue
                            }
                            );
                    }

                    string disciplineValue = eaElement.GetTaggedValueString("Perspective");

                    if (!string.IsNullOrEmpty(disciplineValue))
                    {
                        if (disciplineValue == "User")
                        {
                            elementResource.Properties.Add(new Property()
                            {
                                Class = new Key("PC-Perspective", "1.1"),
                                Value = "V-perspective-1"
                            });
                        }
                    }
                }
                #endregion

                result = elementResource;

                ElementResource resource = new ElementResource
                {
                    Resource = elementResource,
                    EaGUID = eaElement.ElementGUID,
                    Key = new Key(specIfID, revision)
                };

                // ***  convert implicit respources ***





                // diagrams
                EAAPI.Collection diagramCollection;

                if (elementType == "Package")
                {
                    diagramCollection = elementPackage.Diagrams;
                }
                else
                {
                    diagramCollection = eaElement.Diagrams;
                }

                for (short counter = 0; counter < diagramCollection.Count; counter++)
                {
                    EAAPI.Diagram diagram = diagramCollection.GetAt(counter) as EAAPI.Diagram;

                    Key diagramKey = GetKeyForDiagram(diagram);

                    Statement parentStatement = GetContainsStatementFromSpecIfID(resource.Key, diagramKey);

                    resource.ImplicitElementsStatements.Add(parentStatement);
                }

                // packages
                if (elementType == "Package")
                {

                    for (short counter = 0; counter < elementPackage.Packages.Count; counter++)
                    {
                        EAAPI.Package childPackage = elementPackage.Packages.GetAt(counter) as EAAPI.Package;

                        Key packageKey = GetKeyForElement(childPackage.Element);

                        Statement parentStatement = GetContainsStatementFromSpecIfID(resource.Key, packageKey);

                        resource.ImplicitElementsStatements.Add(parentStatement);
                    }

                }

                // classifier
                if (elementType != "Port")
                {
                    Statement classifierStatement = GetClassifierStatement(eaElement);
                    if (classifierStatement != null)
                    {
                        resource.ImplicitElementsStatements.Add(classifierStatement);
                    }
                }


                // tagged values
                List<ImplicitElementResource> taggedValueResources = ConvertElementTaggedValues(eaElement, resource);

                foreach (ImplicitElementResource tag in taggedValueResources)
                {
                    Statement tagValueStatement = GetContainsStatement(resource.Resource, tag.Resource);
                    resource.ImplicitElementsStatements.Add(tagValueStatement);
                }

                // attributes

                // operations

                // constraints

                // alternative ID
                result.AlternativeIDs = new List<object>();

                AlternativeId alternativeId = new AlternativeId()
                {
                    Project = "Enterprise Architect:" + _repository.ProjectGUID,
                    ID = eaElement.ElementGUID,
                    Revision = "1"
                };

                result.AlternativeIDs.Add(alternativeId);

                _elementResources.Add(cacheKey, resource);
            }
            else
            {
                result = _elementResources[cacheKey].Resource;
            }

            return result;
        }

        private List<Statement> GetPortStatements(EAAPI.Element port)
        {
            List<Statement> result = new List<Statement>();

            result.AddRange(ConvertPort(port));

            return result;
        }

        private List<Statement> ConvertPort(EAAPI.Element port)
        {
            List<Statement> result = new List<Statement>();

            string name = port.Name;

            Key portKey = GetKeyForElement(port);

            EAAPI.Element portParent = port.GetParentElement(_repository);
            List<int> outerPortIDs = new List<int>();

            List<int> childPortIDs = new List<int>();

            outerPortIDs.Add(port.ElementID);

            childPortIDs.Add(port.ElementID);

            for (short counter = 0; counter < port.Connectors.Count; counter++)
            {
                EAAPI.Connector connector = port.Connectors.GetAt(counter) as EAAPI.Connector;

                if (connector.Type == "Connector" && connector.Stereotype == "access type")
                {
                    EAAPI.Element connectedPort;
                    bool connectorReversed = false;

                    if (connector.SupplierID == port.ElementID) // destination -> source
                    {
                        connectedPort = _repository.GetElementByID(connector.ClientID);
                        connectorReversed = true;
                    }
                    else
                    {
                        connectedPort = _repository.GetElementByID(connector.SupplierID);
                    }

                    Key connectedPortKey = GetKeyForElement(connectedPort);

                    string connectorDirection;
                    if (connector.Direction == "Source -> Destination")
                    {
                        if (!connectorReversed)
                        {
                            connectorDirection = "out";
                        }
                        else
                        {
                            connectorDirection = "in";
                        }

                    }
                    else if (connector.Direction == "Destination -> Source")
                    {
                        if (!connectorReversed)
                        {
                            connectorDirection = "in";
                        }
                        else
                        {
                            connectorDirection = "out";
                        }
                    }
                    else if (connector.Direction == "Bi-Directional")
                    {
                        connectorDirection = "inout";
                    }
                    else
                    {
                        connectorDirection = "none";
                    }

                    string portStateID = EaSpecIfGuidConverter.CreatePortStateID(port, connectedPort);
                    Key portStateKey = new Key(portStateID, "1");

                    string cacheKey = GetCacheKey(portStateKey);

                    PortStateResource portStateResource;

                    if (!_portStateResources.ContainsKey(cacheKey))
                    {
                        Resource outerPortState = CreatePortConnectionState(portStateID);

                        portStateResource = new PortStateResource
                        {
                            Resource = outerPortState,
                            Key = portStateKey

                        };

                        _portStateResources.Add(cacheKey, portStateResource);

                    }
                    else
                    {
                        portStateResource = _portStateResources[cacheKey];

                    }

                    Statement portStateStatement;
                    Statement statePortStatement;

                    if (connectorDirection == "out")
                    {
                        portStateStatement = GetWritesStatement(portKey, portStateKey);
                        statePortStatement = GetReadsStatement(connectedPortKey, portStateKey);

                    }
                    else if (connectorDirection == "in")
                    {
                        portStateStatement = GetReadsStatement(portKey, portStateKey);
                        statePortStatement = GetWritesStatement(connectedPortKey, portStateKey);
                    }
                    else
                    {
                        portStateStatement = GetStoresStatement(portKey, portStateKey);
                        statePortStatement = GetStoresStatement(portStateKey, connectedPortKey);
                    }

                    string statementKey = GetCacheKey(portStateStatement.ID, portStateStatement.Revision);
                    string oppositeKey = GetCacheKey(statePortStatement.ID, statePortStatement.Revision);

                    if (!portStateResource.Statements.ContainsKey(statementKey))
                    {
                        portStateResource.Statements.Add(statementKey, portStateStatement);
                        portStateResource.Statements.Add(oppositeKey, statePortStatement);
                    }

                    EAAPI.Element portClassifier = port.GetClassifier(_repository);

                    if (portClassifier != null)
                    {

                        Statement portClassifierStatement = GetClassifierStatement(portStateKey, GetKeyForElement(portClassifier));

                        string classifierStatementKey = GetCacheKey(portClassifierStatement.ID, portClassifierStatement.Revision);

                        if (!portStateResource.Statements.ContainsKey(classifierStatementKey))
                        {
                            portStateResource.Statements.Add(classifierStatementKey, portClassifierStatement);
                        }

                    }

                    result.Add(portStateStatement);


                } // if(connector.Type == "Connector" && connector.Stereotype == "access type")
            } // for

            return result;
        }

        private Resource CreatePortConnectionState(string guid)
        {
            Resource result = new Resource()
            {
                ID = guid,
                Revision = "1",
                Class = new Key("RC-State", "1.1")

            };

            return result;
        }

        private List<ImplicitElementResource> ConvertElementTaggedValues(EAAPI.Element eaElement, ElementResource parent)
        {
            List<ImplicitElementResource> result = new List<ImplicitElementResource>();

            string revision = EaDateToRevisionConverter.ConvertDateToRevision(eaElement.Modified);

            string namespc = "";
            string stereotype = "";

            ParseFullQualifiedName(eaElement.FQStereotype, out namespc, out stereotype);

            for (short counter = 0; counter < eaElement.TaggedValues.Count; counter++)
            {

                EAAPI.TaggedValue tag = eaElement.TaggedValues.GetAt(counter) as EAAPI.TaggedValue;

                string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID);


                string tagNamespace = "";
                string tagStereotype = "";
                string tagName = tag.Name;

                if (!string.IsNullOrEmpty(tag.FQName))
                {
                    ParseFullQualifiedTagName(tag.FQName, out tagNamespace, out tagStereotype, out tagName);
                }

                Resource tagResource = new Resource()
                {
                    ID = specIfID,
                    Class = new Key("RC-State", "1.1"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author,
                    Revision = revision
                };

                tagResource.Properties = new List<Property>();

                tagResource.Properties.Add(new Property(new Key("PC-Name", "1.1"), tagName));

                tagResource.Properties.Add(new Property(new Key("PC-Description", "1.1"), tag.Notes));

                tagResource.Properties.Add(new Property(new Key("PC-Type", "1.1"), "OMG:UML:2.5.1:TaggedValue"));


                tagResource.Properties.Add(new Property(new Key("PC-Value", "1.1"), eaElement.GetTaggedValueString(tag.Name)));

                string stereotypeValue = "";

                if (tagStereotype != "")
                {
                    stereotypeValue = tagNamespace + ":" + tagStereotype;
                }
                else
                {
                    stereotypeValue = stereotype;
                }

                tagResource.Properties.Add(new Property(new Key("PC-Stereotype", "1.1"), stereotypeValue));



                ImplicitElementResource tagValueResource = new ImplicitElementResource
                {
                    Resource = tagResource,
                    Key = new Key(tagResource.ID, tagResource.Revision),
                    Parent = new Key(parent.Resource.ID, parent.Resource.Revision),
                    ParentEaGUID = parent.EaGUID
                };

                result.Add(tagValueResource);

                _implicitElementResources.Add(GetCacheKey(tagResource), tagValueResource);
            }


            return result;
        }

        private Resource ConvertElementConstraint(EAAPI.Element element, EAAPI.Constraint elementConstraint, int index)
        {
            Resource result;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID + "_CONSTRAINT_" + index);

            if (!_resources.ContainsKey(specIfID))
            {

                result = new Resource()
                {
                    ID = specIfID,
                    Class = new Key("RC-State", "1.1"),
                    Revision = "1"
                };

                result.Properties = new List<Property>();

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Name", "1.1"),
                        Value = elementConstraint.Name

                    }
                    );

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Type", "1.1"),
                        Value = "OMG:UML:2.5.1:Constraint"

                    }
                    );

                result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Stereotype", "1.1"),
                            Value = elementConstraint.Type
                        }
                        );

                result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Value", "1.1"),
                            Value = elementConstraint.Notes
                        }
                        );
            }
            else
            {
                result = _resources[specIfID];
            }

            return result;
        }

        private Resource ConvertOperation(EAAPI.Method operation, EAAPI.Element eaElement, string elementRevision)
        {

            Resource result;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID);

            if (!_resources.ContainsKey(specIfID))
            {

                Resource operationResource = new Resource()
                {
                    ID = specIfID,
                    Class = new Key("RC-Actor", "1.1"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author,
                    Revision = elementRevision
                };

                operationResource.Properties = new List<Property>();

                operationResource.Properties.Add(
                    new Property(new Key("PC-Name", "1.1"), operation.Name));


                operationResource.Properties.Add(
                    new Property(new Key("PC-Description", "1.1"), operation.Notes));


                operationResource.Properties.Add(
                    new Property(new Key("PC-Type", "1.1"), "OMG:UML:2.5.1:Operation"));


                string namespc = "";
                string stereotype = "";

                ParseFullQualifiedName(operation.FQStereotype, out namespc, out stereotype);

                string stereotypeValue = "";

                if (namespc != "EAUML" && !string.IsNullOrWhiteSpace(namespc))
                {
                    stereotypeValue = namespc + ":";
                }
                else if (namespc == "EAUML" && stereotype != "")
                {
                    stereotypeValue = "UML:";
                }

                stereotypeValue += stereotype;

                if (operation.ReturnType == "entry")
                {
                    if (stereotypeValue != "")
                    {
                        stereotypeValue += ",";
                    }
                    stereotypeValue += "entry";
                }
                else if (operation.ReturnType == "exit")
                {
                    if (stereotypeValue != "")
                    {
                        stereotypeValue += ",";
                    }
                    stereotypeValue += "exit";
                }
                else if (operation.ReturnType == "do")
                {
                    if (stereotypeValue != "")
                    {
                        stereotypeValue += ",";
                    }
                    stereotypeValue += "do";
                }

                operationResource.Properties.Add(new Property(new Key("PC-Stereotype", "1.1"), stereotypeValue));


                string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID) + "_VISIBILITY";

                operationResource.Properties.Add(GetVisibilityProperty(operation.Visibility, id, elementRevision, eaElement.Modified, eaElement.Author));

                result = operationResource;
            }
            else
            {
                result = _resources[specIfID];
            }

            return result;
        }

        private Resource ConvertOperationParameter(EAAPI.Parameter parameter, EAAPI.Element eaElement, string elementRevision)
        {
            Resource result;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID);

            if (!_resources.ContainsKey(specIfID))
            {
                result = new Resource()
                {
                    ID = specIfID,
                    Class = new Key("RC-State", "1.1"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author,
                    Revision = elementRevision
                };

                result.Properties = new List<Property>();

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Name", "1.1"),
                        Value = parameter.Name
                    }
                    );

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Description", "1.1"),
                        Value = parameter.Notes
                    }
                    );

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Type", "1.1"),
                        Value = "OMG:UML:2.5.1:Parameter"
                    }
                    );
            }
            else
            {
                result = _resources[specIfID];
            }

            return result;
        }

        private Resource ConvertAttribute(EAAPI.Attribute attribute, EAAPI.Element eaElement, string elementRevision)
        {

            Resource result;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID);

            if (!_resources.ContainsKey(specIfID))
            {
                result = new Resource()
                {
                    ID = specIfID,
                    Class = new Key("RC-State", "1.1"),
                    ChangedAt = eaElement.Modified,
                    ChangedBy = eaElement.Author,
                    Revision = elementRevision,

                };

                result.Properties = new List<Property>();

                result.Properties.Add(new Property(new Key("PC-Name", "1.1"), attribute.Name));


                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Description", "1.1"),
                        Value = attribute.Notes
                    }
                    );

                result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Type", "1.1"),
                        Value = "OMG:UML:2.5.1:Attribute"
                    }
                    );

                result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Value", "1.1"),
                            Value = attribute.Default
                        }
                        );

                string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID) + "_VISIBILITY";

                result.Properties.Add(GetVisibilityProperty(attribute.Visibility, id, elementRevision, eaElement.Modified, eaElement.Author));
            }
            else
            {
                result = _resources[specIfID];
            }

            return result;
        }

        private List<Statement> ConvertExplicitElementStatements(EAAPI.Element eaElement)
        {
            List<Statement> result = new List<Statement>();

            Key resourceKey = GetKeyForElement(eaElement);

            string elementType = eaElement.Type;

            EAAPI.Package elementPackage;

            if (elementType == "Package")
            {
                elementPackage = _repository.GetPackageByGuid(eaElement.ElementGUID);
            }
            else
            {
                elementPackage = _repository.GetPackageByID(eaElement.PackageID);
            }

            // contains (parent element)
            if (eaElement.ParentID != 0)
            {
                EAAPI.Element parentElement = _repository.GetElementByID(eaElement.ParentID);

                if (parentElement != null)
                {
                    string parentSpecifID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parentElement.ElementGUID);
                    string parentRevision = EaDateToRevisionConverter.ConvertDateToRevision(parentElement.Modified);

                    Statement parentStatement = GetContainsStatementFromSpecIfID(new Key(parentSpecifID, parentRevision), resourceKey);

                    result.Add(parentStatement);

                }
            }
            else
            {
                if (elementType != "Package")
                {

                    Key packageKey = GetKeyForElement(elementPackage.Element);

                    Statement parentStatement = GetContainsStatementFromSpecIfID(packageKey, resourceKey);

                    result.Add(parentStatement);

                }
                else
                {
                    EAAPI.Package parentPackage = _repository.GetPackageByID(eaElement.PackageID);

                    if (parentPackage != null && parentPackage.Element != null)
                    {

                        Key packageKey = GetKeyForElement(parentPackage.Element);

                        Statement parentStatement = GetContainsStatementFromSpecIfID(packageKey, resourceKey);

                        result.Add(parentStatement);
                    }
                }
            }

            // child elements

            EAAPI.Collection elementCollection;

            if (elementType == "Package")
            {
                elementCollection = elementPackage.Elements;
            }
            else
            {
                elementCollection = eaElement.Elements;
            }

            for (short counter = 0; counter < elementCollection.Count; counter++)
            {
                EAAPI.Element childElement = elementCollection.GetAt(counter) as EAAPI.Element;

                string childElementType = childElement.Type;

                if (childElementType != "Port" && childElementType != "ActionPin")
                {
                    Key childKey = GetKeyForElement(childElement);

                    Statement childStatement = GetContainsStatementFromSpecIfID(resourceKey, childKey);

                    result.Add(childStatement);
                }

            }

            // embedded elements
            for (short counter = 0; counter < eaElement.EmbeddedElements.Count; counter++)
            {
                EAAPI.Element embeddedElement = eaElement.EmbeddedElements.GetAt(counter) as EAAPI.Element;

                Key childKey = GetKeyForElement(embeddedElement);

                Statement childStatement = GetContainsStatementFromSpecIfID(resourceKey, childKey);

                result.Add(childStatement);
            }

            // shows (diagrams)
            CachedDiagramDataProvider cachedDiagramDataProvider = new CachedDiagramDataProvider(_repository);

            List<int> diagramIDs = cachedDiagramDataProvider.GetAllDiagramIDsForElement(eaElement.ElementID);

            foreach (int diagramID in diagramIDs)
            {
                EAAPI.Diagram diagram = _repository.GetDiagramByID(diagramID);

                if (diagram != null)
                {
                    string diagramSpecifID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
                    string diagramRevision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

                    Key diagramKey = new Key(diagramSpecifID, diagramRevision);

                    Statement showsStatement = GetShowsStatementFromSpecIfID(diagramKey, resourceKey);

                    result.Add(showsStatement);
                }

            }

            if (elementType == "Port")
            {
                result.AddRange(GetPortStatements(eaElement));
            }

            // external connectors
            result.AddRange(ConvertExternalConnectors(eaElement));



            return result;
        }

        private List<Statement> ConvertExternalConnectors(EAAPI.Element eaElement)
        {
            List<Statement> result = new List<Statement>();

            for (short counter = 0; counter < eaElement.Connectors.Count; counter++)
            {
                EAAPI.Connector connectorEA = eaElement.Connectors.GetAt(counter) as EAAPI.Connector;

                EAAPI.Element sourceElement = _repository.GetElementByID(connectorEA.ClientID);
                EAAPI.Element targetElement = _repository.GetElementByID(connectorEA.SupplierID);


                if (connectorEA.Direction == "Destination -> Source")
                {
                    sourceElement = _repository.GetElementByID(connectorEA.SupplierID);
                    targetElement = _repository.GetElementByID(connectorEA.ClientID);
                }


                Key sourceKey = GetKeyForElement(sourceElement);
                Key targetKey = GetKeyForElement(targetElement);

                string sourceElementType = sourceElement.Type;
                string targetElementType = targetElement.Type;
                string connectorType = connectorEA.Type;
                string connectorStereotype = connectorEA.Stereotype;

                Key subjectKey = sourceKey;
                Key objectKey = targetKey;

                if (connectorType == "Dependency")
                {
                    if (connectorStereotype == "satisfy")
                    {
                        result.Add(GetSatisfiesStatement(subjectKey, objectKey));
                    }
                    else if (connectorStereotype == "deriveReqt")
                    {
                        result.Add(GetRefinesStatement(subjectKey, objectKey));
                    }
                }
                else if (connectorType == "Aggregation" && connectorEA.Subtype == "Strong")
                {
                    result.Add(GetContainsStatementFromSpecIfID(objectKey, subjectKey));
                }


            }

            return result;
        }

        private List<Statement> ConvertExplicitDiagramStatements(EAAPI.Diagram diagram)
        {
            List<Statement> result = new List<Statement>();

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
            string revision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

            Key diagramKey = new Key(specIfID, revision);

            if (diagram.ParentID != 0)
            {
                EAAPI.Element parentElement = _repository.GetElementByID(diagram.ParentID);

                if (parentElement != null)
                {
                    string parentSpecifID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parentElement.ElementGUID);
                    string parentRevision = EaDateToRevisionConverter.ConvertDateToRevision(parentElement.Modified);

                    Statement parentStatement = GetContainsStatementFromSpecIfID(new Key(parentSpecifID, parentRevision), diagramKey);

                    result.Add(parentStatement);

                }
            }
            else
            {
                EAAPI.Package diagramPackage = _repository.GetPackageByID(diagram.PackageID);

                Key packageKey = GetKeyForElement(diagramPackage.Element);

                Statement parentStatement = GetContainsStatementFromSpecIfID(packageKey, diagramKey);

                result.Add(parentStatement);
            }

            return result;
        }



        private Resource ConvertConnectorConstraint(EAAPI.Connector connector, EAAPI.ConnectorConstraint connectorConstraint, int index)
        {
            Resource result = new Resource()
            {
                ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connector.ConnectorGUID + "_CONSTRAINT_" + index),
                Class = new Key("RC-State", "1.1"),
                Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID()
            };

            result.Properties = new List<Property>();

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Name", "1.1"),
                    Value = connectorConstraint.Name

                }
                );

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Type", "1.1"),
                    Value = "OMG:UML:2.5.1:Constraint"

                }
                );

            result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Stereotype", "1.1"),
                        Value = connectorConstraint.Type
                    }
                    );

            result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Value", "1.1"),
                        Value = connectorConstraint.Notes
                    }
                    );

            return result;
        }

        private Resource ConvertConnectorGuard(EAAPI.Connector connector)
        {
            Resource result = new Resource()
            {
                ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connector.ConnectorGUID + "_GUARD"),
                Class = new Key("RC-Actor", "1.1"),
                Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID()
            };

            result.Properties = new List<Property>();

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Name", "1.1"),
                    Value = connector.TransitionGuard

                }
                );

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Type", "1.1"),
                    Value = "OMG:UML:2.5.1:Constraint"

                }
                );

            result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Stereotype", "1.1"),
                        Value = "Guard"
                    }
                    );

            result.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Value", "1.1"),
                        Value = connector.TransitionGuard
                    }
                    );

            return result;
        }

        private List<Resource> ConvertConnectorTaggedValues(EAAPI.Connector eaConnector)
        {
            List<Resource> result = new List<Resource>();



            string namespc = "";
            string stereotype = "";

            ParseFullQualifiedName(eaConnector.FQStereotype, out namespc, out stereotype);

            for (short counter = 0; counter < eaConnector.TaggedValues.Count; counter++)
            {
                EAAPI.ConnectorTag tag = eaConnector.TaggedValues.GetAt(counter) as EAAPI.ConnectorTag;

                string tagNamespace = "";
                string tagStereotype = "";
                string tagName = tag.Name;

                ParseFullQualifiedTagName(tag.FQName, out tagNamespace, out tagStereotype, out tagName);

                Resource tagResource = new Resource()
                {
                    ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID),
                    Class = new Key("RC-State", "1.1"),
                    Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID()
                };

                tagResource.Properties = new List<Property>();

                tagResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Name", "1.1"),
                        Value = tagName
                    }
                    );

                tagResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Description", "1.1"),
                        Value = tag.Notes
                    }
                    );

                tagResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Type", "1.1"),
                        Value = "OMG:UML:2.5.1:TaggedValue"
                    }
                    );

                tagResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Value", "1.1"),
                        Value = tag.Value
                    }
                    );

                string stereotypeValue = "";

                if (tagStereotype != "")
                {
                    stereotypeValue = tagNamespace + ":" + tagStereotype;
                }
                else
                {
                    stereotypeValue = stereotype;
                }

                tagResource.Properties.Add(
                    new Property()
                    {
                        Class = new Key("PC-Stereotype", "1.1"),
                        Value = stereotypeValue
                    }
                    );

                result.Add(tagResource);
            }


            return result;
        }

        public Resource ConvertDiagram(EAAPI.Diagram diagram)
        {
            Resource result = null;

            string cacheKey = GetCacheKeyForDiagram(diagram);

            if (!_diagramResources.ContainsKey(cacheKey))
            {
                string diagramXHTML = "";

                if (diagram != null)
                {

                    string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
                    string revision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);




                    #region DIAGRAM_RESOURCE_CRAETION

                    //               try
                    //               {

                    //	string diagramFileName = System.IO.Path.GetTempPath() + "/" + diagram.DiagramGUID + ".png";

                    //	_repository.GetProjectInterface().PutDiagramImageToFile(diagram.DiagramGUID, diagramFileName, 1);

                    //	Image image = Image.FromFile(diagramFileName);

                    //	using (MemoryStream m = new MemoryStream())
                    //	{
                    //		image.Save(m, image.RawFormat);

                    //		byte[] imageBytes = m.ToArray();

                    //		// Convert byte[] to Base64 String
                    //		string base64String = Convert.ToBase64String(imageBytes);

                    //		diagramXHTML += "<p><img style=\"max-width:100%\" src=\"data:image/png;base64," + base64String + "\" /></p>";
                    //		m.Close();
                    //		image.Dispose();
                    //	}
                    //}
                    //catch (Exception exception)
                    //{

                    //}

                    #region SVG
                    EaSvgMetadataProvider eaSvgMetadataProvider = new EaSvgMetadataProvider();

                    DiagramToSvgConverter diagramToSvgConverter = new DiagramToSvgConverter(_repository, eaSvgMetadataProvider);

                    ScalableVectorGraphics svgDiagram = diagramToSvgConverter.ConvertDiagramToSVG(diagram);

                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("di", "http://www.omg.org/spec/DD/20100524/DI");
                    namespaces.Add("dc", "http://www.omg.org/spec/DD/20100524/DC");
                    namespaces.Add("specif", "https://specif.de/schema/v1.1/DI");

                    // Create a Type array.
                    Type[] extraTypes = new Type[2];
                    extraTypes[0] = typeof(Shape);
                    extraTypes[1] = typeof(Edge);
                    //extraTypes[2] = typeof(MetadataContent);
                    //extraTypes[0] = typeof(SpecIfDiagramInterchangeBase);

                    // Each overridden field, property, or type requires
                    // an XmlAttributes instance.  
                    XmlAttributes xmlSerializationAttributes = new XmlAttributes();

                    // Creates an XmlElementAttribute instance to override the
                    // field that returns Book objects. The overridden field  
                    // returns Expanded objects instead.  
                    XmlElementAttribute metadataElementAttribute = new XmlElementAttribute();
                    metadataElementAttribute.ElementName = "metadata";
                    metadataElementAttribute.Type = typeof(SpecIfMetadata);

                    // Adds the element to the collection of elements.  
                    xmlSerializationAttributes.XmlElements.Add(metadataElementAttribute);

                    // Creates the XmlAttributeOverrides instance.  
                    XmlAttributeOverrides attributeOverrides = new XmlAttributeOverrides();

                    // Adds the type of the class that contains the overridden
                    // member, as well as the XmlAttributes instance to override it
                    // with, to the XmlAttributeOverrides.  
                    attributeOverrides.Add(typeof(PresentationElement), "Metadata", xmlSerializationAttributes);

                    // Creates the XmlSerializer using the XmlAttributeOverrides.  
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ScalableVectorGraphics), attributeOverrides);

                    using (var stringWriter = new StringWriter())
                    {
                        using (XmlWriter writer = XmlWriter.Create(stringWriter,
                            new XmlWriterSettings()
                            {
                                OmitXmlDeclaration = true,
                                ConformanceLevel = ConformanceLevel.Auto,
                                Indent = false
                            }))
                        {

                            xmlSerializer.Serialize(writer, svgDiagram, namespaces);
                            diagramXHTML = stringWriter.ToString();

                            //string svg = "<svg><circle cx=\"50\" cy=\"50\" r=\"40\" stroke=\"black\" stroke-width=\"3\" fill=\"red\" /></svg>";

                            //diagramXHTML = "<p>" + svg + "hallo SVG?</p>";



                            //System.IO.File.WriteAllText("d:/test/svg/svgx.svg", diagramXHTML);
                        }
                    }

                    #endregion



                    result = new Resource()
                    {
                        ID = specIfID,
                        Class = new Key("RC-Diagram", "1.1"),
                        ChangedAt = diagram.ModifiedDate,
                        ChangedBy = diagram.Author,
                        Revision = revision
                    };

                    result.Properties = new List<Property>();

                    result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Name", "1.1"),
                            Value = diagram.Name
                        }
                        );

                    result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Description", "1.1"),
                            Value = diagram.Notes
                        }
                        );

                    result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Diagram", "1.1"),
                            MultilanguageValue = new MultilanguageText
                            {
                                Text = diagramXHTML,
                                Format = "xhtml"
                            }
                        });

                    result.Properties.Add(
                        new Property()
                        {
                            Class = new Key("PC-Type", "1.1"),
                            Value = "OMG:UML:2.5.1:" + GetUmlDiagramTypeFromEaType(diagram.Type)
                        }
                        );

                    #endregion

                    DiagramResource diagramResource = new DiagramResource
                    {
                        Resource = result,
                        EaGUID = diagram.DiagramGUID,
                        Key = new Key(specIfID, revision)
                    };

                    // shows statements

                    CachedDiagramDataProvider cachedDiagramDataProvider = new CachedDiagramDataProvider(_repository);

                    List<int> elementIDs = cachedDiagramDataProvider.GetShownElementIDs(diagram.DiagramID);

                    foreach (int elementID in elementIDs)
                    {
                        EAAPI.Element elementOnDiagram = _repository.GetElementByID(elementID);

                        if (elementOnDiagram != null)
                        {
                            Key elementOnDiagramKey = GetKeyForElement(elementOnDiagram);

                            Statement showsStatement = GetShowsStatementFromSpecIfID(diagramResource.Key, elementOnDiagramKey);

                            diagramResource.ImplicitDiagramStatements.Add(showsStatement);
                        }

                    }

                    // contains statement

                    EAAPI.Element parentElement = null;

                    if (diagram.ParentID != 0)
                    {
                        parentElement = _repository.GetElementByID(diagram.ParentID);
                    }
                    else
                    {
                        EAAPI.Package diagramPackage = _repository.GetPackageByID(diagram.PackageID);

                        parentElement = diagramPackage.Element;
                    }

                    if (parentElement != null)
                    {
                        string parentSpecifID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parentElement.ElementGUID);
                        string parentRevision = EaDateToRevisionConverter.ConvertDateToRevision(parentElement.Modified);

                        Resource parentResource = GetResourceByKey(new Key(parentSpecifID, parentRevision));

                        Statement containsStatement = GetContainsStatement(parentResource, result);

                        diagramResource.ImplicitDiagramStatements.Add(containsStatement);
                    }

                    _diagramResources.Add(cacheKey, diagramResource);

                }
            }
            else
            {
                result = _diagramResources[cacheKey].Resource;
            }
            return result;
        }

        private Statement GetClassifierStatement(Resource classifier, Resource classifiedResource)
        {
            Statement result = null;




            return result;
        }

        private Statement GetSatisfiesStatement(Key subjectResource, Key objectResource)
        {
            Statement result;

            string statementID = subjectResource.ID + "_SATISFIES_" + objectResource.ID;

            // rdf:type statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-satisfies", "1.1"),
                StatementSubject = new Key(subjectResource.ID, subjectResource.Revision),
                StatementObject = new Key(objectResource.ID, objectResource.Revision)
            };


            return result;
        }

        private Statement GetContainsStatement(Resource parentResource, Resource childResource)
        {
            Statement result = null;


            // SpecIF:contains statement
            result = GetContainsStatementFromSpecIfID(new Key(parentResource.ID, parentResource.Revision),
                                                      new Key(childResource.ID, childResource.Revision));

            return result;
        }

        private Statement GetContainsStatementFromSpecIfID(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_CONTAINS_" + objectID.ID;

            // SpecIF:contains statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-contains", "1.1"),
                StatementSubject = subjectID,
                StatementObject = objectID
            };

            return result;
        }

        private Statement GetShowsStatementFromSpecIfID(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_SHOWS_" + objectID.ID;

            // SpecIF:shows statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-shows", "1.1"),
                StatementSubject = new Key(subjectID.ID, subjectID.Revision),
                StatementObject = new Key(objectID.ID, objectID.Revision)
            };

            return result;
        }

        private Statement GetWritesStatement(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_WRITES_" + objectID.ID;

            // SpecIF:shows statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-writes", "1.1"),
                StatementSubject = new Key(subjectID.ID, subjectID.Revision),
                StatementObject = new Key(objectID.ID, objectID.Revision)
            };

            return result;
        }

        private Statement GetReadsStatement(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_READS_" + objectID.ID;

            // SpecIF:shows statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-reads", "1.1"),
                StatementSubject = new Key(subjectID.ID, subjectID.Revision),
                StatementObject = new Key(objectID.ID, objectID.Revision)
            };

            return result;
        }

        private Statement GetStoresStatement(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_STORES_" + objectID.ID;

            // SpecIF:shows statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-stores", "1.1"),
                StatementSubject = new Key(subjectID.ID, subjectID.Revision),
                StatementObject = new Key(objectID.ID, objectID.Revision)
            };

            return result;
        }

        private Statement GetRefinesStatement(Key subjectID, Key objectID)
        {
            Statement result = null;

            string statementID = subjectID.ID + "_REFINES_" + objectID.ID;

            // IREB:refines statement
            result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-refines", "1.1"),
                StatementSubject = new Key(subjectID.ID, subjectID.Revision),
                StatementObject = new Key(objectID.ID, objectID.Revision)
            };

            return result;
        }


        private List<Statement> GetSubelementStatements(EAAPI.Element eaElement, Key elementResource)
        {
            List<Statement> result = new List<Statement>();

            string type = eaElement.Type;

            for (short counter = 0; counter < eaElement.Elements.Count; counter++)
            {
                EAAPI.Element subElement = eaElement.Elements.GetAt(counter) as EAAPI.Element;

                string subelementSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(subElement.ElementGUID);



                // add UML:AtributeReference statement
                Statement attributeReferenceStatement = GetContainsStatementFromSpecIfID(
                    new Key(elementResource.ID, elementResource.Revision),
                    new Key(subelementSpecIfID, "1.1")
                    );



                result.Add(attributeReferenceStatement);

            } // for


            for (short counter = 0; counter < eaElement.EmbeddedElements.Count; counter++)
            {
                EAAPI.Element subElement = eaElement.EmbeddedElements.GetAt(counter) as EAAPI.Element;

                string subElementSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(subElement.ElementGUID);

                // add UML:AtributeReference statement
                Statement attributeReferenceStatement = GetContainsStatementFromSpecIfID(
                    new Key(elementResource.ID, elementResource.Revision),
                    new Key(subElementSpecIfID, "1.1")
                    );

                result.Add(attributeReferenceStatement);

            } // for

            return result;
        }

        private Statement GetClassifierStatement(EAAPI.Element element)
        {
            Statement result = null;

            string elementType = element.Type;

            string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID);
            string specifRevision = EaDateToRevisionConverter.ConvertDateToRevision(element.Modified);
            // classifier

            int classifierID = 0;

            if (elementType == "Port" || elementType == "Part" || elementType == "ActionPin" || elementType == "ActivityParameter")
            {
                classifierID = element.PropertyType;
            }
            else
            {
                classifierID = element.ClassifierID;
            }

            if (classifierID != 0)
            {

                EAAPI.Element classifierElement = element.GetClassifier(_repository);

                string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);
                string classifierRevision = EaDateToRevisionConverter.ConvertDateToRevision(classifierElement.Modified);

                string statementID = specIfID + "_INSTANCEOF_" + classifierSpecIfID;

                // rdf:type statement
                result = new Statement()
                {
                    ID = statementID,
                    Revision = "1",
                    Class = new Key("SC-Classifier", "1.1"),
                    StatementSubject = new Key(specIfID, specifRevision),
                    StatementObject = new Key(classifierSpecIfID, classifierRevision)
                };

            }

            return result;
        }

        private Statement GetClassifierStatement(Key instanceKey, Key classifierKey)
        {
            string statementID = instanceKey.ID + "_INSTANCEOF_" + classifierKey.ID;

            Statement result = new Statement()
            {
                ID = statementID,
                Revision = "1",
                Class = new Key("SC-Classifier", "1.1"),
                StatementSubject = instanceKey,
                StatementObject = classifierKey
            };

            return result;
        }

        #endregion

        private void ParseFullQualifiedName(string fullQualifiedName, out string namespc, out string name)
        {
            if (fullQualifiedName.Contains("::"))
            {
                string[] separator = { "::" };

                string[] tokens = fullQualifiedName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 2)
                {
                    namespc = tokens[0];
                    name = tokens[1];
                }
                else
                {
                    namespc = "";
                    name = fullQualifiedName;
                }
            }
            else
            {
                namespc = "";
                name = fullQualifiedName;
            }
        }

        private void ParseFullQualifiedTagName(string fullQualifiedName, out string namespc, out string stereotype, out string name)
        {
            stereotype = "";

            if (fullQualifiedName.Contains("::"))
            {
                string[] separator = { "::" };

                string[] tokens = fullQualifiedName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length == 3)
                {
                    namespc = tokens[0];
                    stereotype = tokens[1];
                    name = tokens[2];
                }
                else
                {
                    namespc = "";
                    name = fullQualifiedName;
                }
            }
            else
            {
                namespc = "";
                name = fullQualifiedName;
            }
        }

        private Property GetVisibilityProperty(string visibility, string ID, string revision, DateTime changedAt = default(DateTime), string changedBy = "")
        {
            Property result = new Property()
            {
                Class = new Key("PC-UML_VisibilityKind", revision),
                Values = new List<Value>()
            };

            Value languageValue = new Value("V-UML_VisibilityKind-0");


            switch (visibility.ToLower())
            {
                case "public":
                    languageValue = new Value("V-UML_VisibilityKind-0");
                    break;

                case "private":
                    languageValue = new Value("V-UML_VisibilityKind-1");
                    break;

                case "protected":
                    languageValue = new Value("V-UML_VisibilityKind-2");
                    break;

                case "package":
                    languageValue = new Value("V-UML_VisibilityKind-3");
                    break;

                case "internal":
                    languageValue = new Value("V-UML_VisibilityKind-4");
                    break;

                case "protected internal":
                    languageValue = new Value("V-UML_VisibilityKind-5");
                    break;
            }

            result.Values.Add(languageValue);

            return result;
        }

        private ResourceClass GetResourceClassForElementType(string type, string stereotype = "")
        {
            ResourceClass result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1.1"));

            switch (type)
            {
                case "Class":
                case "Component":
                case "State":
                case "TaggedValue":
                case "Stereotype":
                case "Parameter":
                case "RunState":
                case "ActionPin":
                case "ActivityParameter":
                case "PrimitiveType":

                    result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1.1"));
                    break;

                case "Object":
                    result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1.1"));

                    if (stereotype == "heading")
                    {
                        result = _metadataReader.GetResourceClassByKey(new Key("RC-Folder", "1.1"));
                    }
                    else if (stereotype == "agent")
                    {
                        result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1.1"));
                    }
                    break;


                case "Port":
                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1.1"));
                    break;

                case "Activity":
                case "Action":
                case "Actor":
                case "StateMachine":
                case "UseCase":
                case "Decision":

                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1.1"));
                    break;

                case "Package":

                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Collection", "1.1"));

                    if (stereotype == "specification")
                    {
                        result = _metadataReader.GetResourceClassByKey(new Key("RC-Hierarchy", "1.1"));
                    }
                    break;

                case "Requirement":
                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Requirement", "1.1"));
                    break;

                case "StateNode":
                case "Event":
                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Event", "1.1"));
                    break;
            }

            return result;
        }

        private string GetUmlDiagramTypeFromEaType(string eaType)
        {
            string result = "";

            switch (eaType)
            {
                case "Logical":
                    result = "Class Diagram";
                    break;

                case "Object":
                    result = "Object Diagram";
                    break;

                case "Component":
                    result = "Component Diagram";
                    break;

                case "Use Case":
                    result = "Use Case Diagram";
                    break;

                case "Statechart":
                    result = "State Machine Diagram";
                    break;

                case "Activity":
                    result = "Activity Diagram";
                    break;


                default:
                    result = eaType;
                    break;
            }

            return result;
        }

        private string GetDirectionID(string connectorDirection)
        {
            string result = "V-UML_ConnectorDirection-0";

            switch (connectorDirection)
            {
                case "Unspecified":
                    result = "V-UML_ConnectorDirection-0";
                    break;

                case "Source -> Destination":
                    result = "V-UML_ConnectorDirection-1";
                    break;

                case "Destination -> Source":
                    result = "V-UML_ConnectorDirection-2";
                    break;

                case "Bi-Directional":
                    result = "V-UML_ConnectorDirection-3";
                    break;

            }

            return result;
        }

        private string GetStatusEnumID(string status)
        {
            string result = "";

            switch (status)
            {
                case "00_rejected":
                    result = "V-Status-1";
                    break;

                case "10_initial":
                    result = "V-Status-2";
                    break;

                case "20_drafted":
                    result = "V-Status-3";
                    break;

                case "30_submitted":
                    result = "V-Status-4";
                    break;

                case "40_approved":
                    result = "V-Status-5";
                    break;

                case "60_completed":
                    result = "V-Status-6";
                    break;
            }

            return result;
        }

        private Key GetKeyForElement(EAAPI.Element eaElement)
        {
            Key result;

            string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);
            string revision = EaDateToRevisionConverter.ConvertDateToRevision(eaElement.Modified);

            result = new Key(id, revision);

            return result;
        }

        private Key GetKeyForDiagram(EAAPI.Diagram diagram)
        {
            Key result;

            string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);
            string revision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

            result = new Key(id, revision);

            return result;
        }

        private void InitializePrimitiveTypes()
        {

            string intID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(INTEGER_GUID);


            Resource integerResource = CreatePrimitiveTypeResource(
                intID,
                "Integer",
                "An instance of Integer is a value in the (infinite) set of integers (…-2, -1, 0, 1, 2…).");



            if (!_resources.ContainsKey(intID))
            {
                _resources.Add(intID, integerResource);
            }

            string boolID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(BOOLEAN_GUID);

            Resource booleanResource = CreatePrimitiveTypeResource(
                boolID,
                "Boolean",
                "An instance of Boolean is one of the predefined values true and false.");

            if (!_resources.ContainsKey(boolID))
            {
                _resources.Add(EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(BOOLEAN_GUID), booleanResource);
            }

            string stringID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(STRING_GUID);

            Resource stringResource = CreatePrimitiveTypeResource(
                stringID,
                "String",
                @"An instance of String defines a sequence of characters. Character sets may include non-Roman
alphabets.The semantics of the string itself depends on its purpose; it can be a comment,
computational language expression, OCL expression, etc.");

            if (!_resources.ContainsKey(stringID))
            {
                _resources.Add(EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(STRING_GUID), stringResource);
            }

            string unlimitedNaturalID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(UNLIMITED_NATURAL_GUID);

            Resource unlimitedNaturalResource = CreatePrimitiveTypeResource(
                unlimitedNaturalID,
                "UnlimitedNatural",
                @"An instance of UnlimitedNatural is a value in the (infinite) set of natural numbers (0, 1, 2…) plus
unlimited. The value of unlimited is shown using an asterisk (‘*’). UnlimitedNatural values are
typically used to denote the upper bound of a range, such as a multiplicity; unlimited is used
whenever the range is specified to have no upper bound.");

            if (_resources.ContainsKey(unlimitedNaturalID))
            {
                _resources.Add(EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(UNLIMITED_NATURAL_GUID), unlimitedNaturalResource);
            }

            string realID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(REAL_GUID);

            Resource realResource = CreatePrimitiveTypeResource(
                realID,
                "Real",
                @"An instance of Real is a value in the (infinite) set of real numbers. Typically an implementation
will internally represent Real numbers using a floating point standard such as ISO/IEC/IEEE
60559:2011 (whose content is identical to the predecessor IEEE 754 standard).");

            if (!_resources.ContainsKey(realID))
            {
                _resources.Add(EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(REAL_GUID), realResource);
            }
        }

        private Resource CreatePrimitiveTypeResource(string id,
                                                     string title,
                                                     string description)
        {
            Resource result = new Resource()
            {
                ID = id,
                Revision = "A4F2DAB6-66FB-470B-98AD-DA6CA2703087",
                Class = new Key(GetResourceClassForElementType("PrimitiveType").ID, "1.1"),
                ChangedAt = new DateTime(2019, 4, 7),
                ChangedBy = "oalt"

            };

            result.Properties = new List<Property>();

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Name", "1.1"),
                    Value = title
                }
                );

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Description", "1.1"),
                    Value = description
                }
                );

            result.Properties.Add(
                new Property()
                {
                    Class = new Key("PC-Type", "1.1"),
                    Value = "OMG:UML:2.5.1:PrimitiveType"
                }
                );

            return result;

        }

        private Resource GetPrimitiveTypeBySwTypeName(string swTypeName)
        {
            Resource result = null;

            switch (swTypeName)
            {
                case "int":
                    result = _resources[EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(INTEGER_GUID)];
                    break;

                case "bool":
                case "boolean":
                    result = _resources[EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(BOOLEAN_GUID)];
                    break;

                case "string":
                case "String":
                    result = _resources[EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(STRING_GUID)];
                    break;

                case "uint":
                case "uword":
                    result = _resources[EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(UNLIMITED_NATURAL_GUID)];
                    break;

                case "float":
                case "double":
                    result = _resources[EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(REAL_GUID)];
                    break;
            }

            return result;
        }

    }
}
