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
		public DataModels.SpecIF ConvertModelToSpecIF(EAAPI.Package selectedPackage)
		{
			DataModels.SpecIF result = new DataModels.SpecIF();

			result.Title = new Value("UML data extracted from Sparx Enterprise Architect: " + selectedPackage.Name);
			result.Generator = "SpecIFicator";

			Node node = new Node();

			GetModelHierarchyRecursively(selectedPackage, node, null);

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

			if(_resources.ContainsKey(key.ID))
			{
				result = _resources[key.ID];
			}
			if(result == null)
            {
				string cacheKey = key.ID + "_R_" + key.Revision;
				if (_resources.ContainsKey(cacheKey))
                {
					result = _resources[cacheKey];
                }
            }
			
			if(result == null)
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

			Dictionary<string, Statement> foundStatements = new Dictionary<string, Statement>();

			foreach(KeyValuePair<string, Statement> keyValuePair in _statements)
            {
				Statement statement = keyValuePair.Value;

				if(
					(statement.StatementSubject.ID == resourceKey.ID && 
				     statement.StatementSubject.Revision == resourceKey.Revision) 
				    ||
				    (statement.StatementObject.ID == resourceKey.ID &&
				     statement.StatementObject.Revision == resourceKey.Revision)
				   )
                {
					if(!foundStatements.ContainsKey(statement.ID))
                    {
						foundStatements.Add(statement.ID, statement);
						result.Add(statement);
                    }
                }
            }

			return result;
        }

		private Node _hierarchy = null;

		public Node GetHierarchy()
        {
			Node result = null;

			Node node = new Node();

			ResetCache();

			//if (_hierarchy == null)
			//{
				EAAPI.Package package = _repository.GetPackageByGuid("{45A143E0-D43A-4f51-ACA6-FF695EEE3256}");

				GetModelHierarchyRecursively(package, node, null);

				_hierarchy = node;
				result = _hierarchy;

			string json = JsonConvert.SerializeObject(node);

			System.IO.File.WriteAllText(@"d:\test\specif\hierarchy.json", json);

			//}
			//else
   //         {
			//	result = _hierarchy;
   //         }
			return result;
		}

		private void GetModelHierarchyRecursively(EAAPI.Package currentPackage, Node currentNode, Resource parentResource)
		{
			Console.WriteLine("Package: " + currentPackage.Name);


			string packageID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentPackage.Element.ElementGUID);

			

			Resource packageResource = AddElement(currentPackage.Element);

            currentNode.ResourceReference = new Key(packageResource.ID, packageResource.Revision);
            currentNode.Description = new Value("Package: " + currentPackage.Name);

            if(parentResource != null)
            {
                Statement containsStatement = GetContainsStatement(parentResource, packageResource);
				if (!_statements.ContainsKey(containsStatement.ID))
				{
					_statements.Add(containsStatement.ID, containsStatement);
				}
            }

			// diagrams
			for (short diagramCounter = 0; diagramCounter < currentPackage.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentPackage.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

				Resource diagramResource = AddDiagram(diagram);
				Statement containsStatement = GetContainsStatement(packageResource, diagramResource);
				if (!_statements.ContainsKey(containsStatement.ID))
				{
					_statements.Add(containsStatement.ID, containsStatement);
				}

				Node node = new Node()
				{
					ResourceReference = new Key(diagramResource.ID, diagramResource.Revision),
					Description = new Value("Diagram: " + diagram.Name)
				};

				currentNode.Nodes.Add(node);
			}

			// recursive call for child packages
			for (short packageCounter = 0; packageCounter < currentPackage.Packages.Count; packageCounter++)
			{
				EAAPI.Package childPackage = currentPackage.Packages.GetAt(packageCounter) as EAAPI.Package;

				Console.WriteLine("Recursive call: " + childPackage.Name);

				string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID);

				Node childNode = new Node()
				{
				};
				currentNode.Nodes.Add(childNode);

				GetModelHierarchyRecursively(childPackage, childNode, packageResource);
				
			}

			// elements
			for (short elementCounter = 0; elementCounter < currentPackage.Elements.Count; elementCounter++)
			{
				EAAPI.Element element = currentPackage.Elements.GetAt(elementCounter) as EAAPI.Element;

				Console.WriteLine("Element: " + element.Name);

				Resource elementResource = AddElement(element);
				Statement containsStatement = GetContainsStatement(packageResource, elementResource);

				if (!_statements.ContainsKey(containsStatement.ID))
				{
					_statements.Add(containsStatement.ID, containsStatement);
				}

				GetElementHierarchyRecursively(element, currentNode, elementResource);
			}
		}

		private void GetElementHierarchyRecursively(EAAPI.Element currentElement, Node currentNode, Resource currentResource)
		{
			string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentElement.ElementGUID);

			string specIfElemenRevision = EaDateToRevisionConverter.ConvertDateToRevision(currentElement.Modified);

			Node node = new Node()
			{
				ResourceReference = new Key(specIfElementID, specIfElemenRevision),
				Description = new Value("Element: " + currentElement.Name)
			};

			currentNode.Nodes.Add(node);

			// diagrams
			for (short diagramCounter = 0; diagramCounter < currentElement.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentElement.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

				Resource diagramResource = AddDiagram(diagram);
				Statement containsStatement = GetContainsStatement(currentResource, diagramResource);
				if (!_statements.ContainsKey(containsStatement.ID))
				{
					_statements.Add(containsStatement.ID, containsStatement);
				}

				string specIfDiagramID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

				Node diagramNode = new Node()
				{
					ResourceReference = new Key(specIfDiagramID, "1")
				};

				currentNode.Nodes.Add(diagramNode);
			}

			// sub-elements
			for (short counter = 0; counter < currentElement.Elements.Count; counter++)
			{
				EAAPI.Element subElement = currentElement.Elements.GetAt(counter) as EAAPI.Element;

				string subElementType = subElement.Type;

				if (subElementType != "Port" && subElementType != "ActionPin" && subElementType != "ActivityParameter")
				{
					Resource subElementResource = AddElement(subElement);
					Statement containsStatement = GetContainsStatement(currentResource, subElementResource);
					if (!_statements.ContainsKey(containsStatement.ID))
					{
						_statements.Add(containsStatement.ID, containsStatement);
					}

					// recursive call
					GetElementHierarchyRecursively(subElement, node, subElementResource);
				}
				
			}

			// embedded elements
			for (short embeddedElementCounter = 0; embeddedElementCounter < currentElement.EmbeddedElements.Count; embeddedElementCounter++)
			{
				EAAPI.Element embeddedElement = currentElement.EmbeddedElements.GetAt(embeddedElementCounter) as EAAPI.Element;

				Console.WriteLine("Embedded element: " + embeddedElement.Name);

				Resource embeddedElementResource = AddElement(embeddedElement);
				Statement containsStatement = GetContainsStatement(currentResource, embeddedElementResource);

				if (!_statements.ContainsKey(containsStatement.ID))
				{
					_statements.Add(containsStatement.ID, containsStatement);
				}

				Node embeddedElementNode = new Node()
				{
					ResourceReference = new Key(embeddedElementResource.ID, embeddedElementResource.Revision),
					Description = new Value("Embedded element: " + embeddedElement.Name)
				};

				node.Nodes.Add(embeddedElementNode);
			}

			
		}

		#region DATA_ADDITION

		private string GetCacheKey(Resource resource)
        {
			string result = resource.ID + "_R_" + resource.Revision;

			return result;
        }

		private string GetCacheKeyForElement(EAAPI.Element element)
        {
			string result = "";

			result += EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID);

			result += "_R_" + EaDateToRevisionConverter.ConvertDateToRevision(element.Modified);

			return result;
        }

		private Resource AddElement(EAAPI.Element eaElement)
		{
			Resource result = ConvertElement(eaElement);

			if (_resources.ContainsKey(GetCacheKey(result)))
			{
				Console.WriteLine("Element still in collection " + result.ID);
			}
			else
			{
				_resources.Add(GetCacheKey(result), result);

				AddClassifier(eaElement, result);

				AddTaggedValues(eaElement, result);

				AddAttributes(eaElement, result);

				AddOperations(eaElement, result);

				AddElementConstraints(eaElement, result);

				AddUmlConnectors(eaElement);
			}



			return result;
		}

		private void AddClassifier(EAAPI.Element element, Resource elementResource)
		{
			string elementType = element.Type;

			string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID);

			// classifier
			
			Statement classifierStatement = null;

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

                Resource classifierResource = null;

				string cacheKey = GetCacheKeyForElement(classifierElement);

				if (!_resources.ContainsKey(cacheKey))
				{
					classifierResource = AddElement(classifierElement);

					classifierStatement = GetClassifierStatement(classifierResource, elementResource);

					if (classifierStatement != null && !_statements.ContainsKey(classifierStatement.ID))
					{
						_statements.Add(classifierStatement.ID, classifierStatement);
					}
				}
                else
                {
                    classifierResource = _resources[cacheKey];

					classifierStatement = GetClassifierStatement(classifierResource, elementResource);

					if (classifierStatement != null && !_statements.ContainsKey(classifierStatement.ID))
					{
						_statements.Add(classifierStatement.ID, classifierStatement);
					}
				}

				
			}
			
		}

		private void AddTaggedValues(EAAPI.Element eaElement, Resource elementResource)
		{
			List<Resource> tagResources = ConvertElementTaggedValues(eaElement);

			foreach (Resource tagResource in tagResources)
			{
				if (!_resources.ContainsKey(tagResource.ID))
				{
					_resources.Add(tagResource.ID, tagResource);
				}

				Statement tagContainsStatement = GetContainsStatement(elementResource, tagResource);

				if (!_statements.ContainsKey(tagContainsStatement.ID))
				{
					_statements.Add(tagContainsStatement.ID, tagContainsStatement);
				}
			}
		}

		private void AddAttributes(EAAPI.Element eaElement, Resource elementResource)
		{
			for(short counter = 0; counter < eaElement.Attributes.Count; counter++ )
			{
				EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

				if(attribute != null)
				{
					Resource attributeResource = ConvertAttribute(attribute, eaElement, elementResource.Revision);

					if (!_resources.ContainsKey(attributeResource.ID))
					{
						_resources.Add(attributeResource.ID, attributeResource);
					}

					// add contains statement
					Statement containsStatement = GetContainsStatement(elementResource, attributeResource);
					if (!_statements.ContainsKey(containsStatement.ID))
					{
						_statements.Add(containsStatement.ID, containsStatement);
					}

					if (attribute.ClassifierID == 0 && !string.IsNullOrEmpty(attribute.Type))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(attribute.Type);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = GetClassifierStatement(primitiveClassifier, attributeResource);

							if (!_statements.ContainsKey(classifierStatement.ID))
							{
								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
					}
					else if (attribute.ClassifierID != 0)
					{
						EAAPI.Element classifierElement = attribute.GetClassifier(_repository);

						string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);

						Resource classifierResource = null;

						if (!_resources.ContainsKey(classifierSpecIfID))
						{
							classifierResource = AddElement(classifierElement);
						}
						else
						{
							classifierResource = _resources[classifierSpecIfID];
						}


						Statement classifierStatement = GetClassifierStatement(classifierResource, attributeResource);

						if (!_statements.ContainsKey(classifierStatement.ID))
						{
							_statements.Add(classifierStatement.ID, classifierStatement);
						}
					}
					
				}
			}

		}

		private void AddOperations(EAAPI.Element eaElement, Resource elementResource)
		{
			for (short counter = 0; counter < eaElement.Methods.Count; counter++)
			{
				EAAPI.Method operation = eaElement.Methods.GetAt(counter) as EAAPI.Method;

				if (operation != null)
				{
					Resource operationResource = ConvertOperation(operation, eaElement, elementResource.Revision);
					
					if (!_resources.ContainsKey(operationResource.ID))
					{
						_resources.Add(operationResource.ID, operationResource);
					}

					Statement containsStatement = GetContainsStatement(elementResource, operationResource);

					if (!_statements.ContainsKey(containsStatement.ID))
					{
						_statements.Add(containsStatement.ID, containsStatement);
					}

					Console.WriteLine("Operation " + operation.Name + " classifierID=" + operation.ClassifierID);

					if (operation.ClassifierID == "0" && !string.IsNullOrEmpty(operation.ReturnType))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(operation.ReturnType);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = GetClassifierStatement(primitiveClassifier, operationResource);
							
							if (!_statements.ContainsKey(classifierStatement.ID))
							{
								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
					}
					else if (operation.ClassifierID != "0")
					{
						int classifierID = int.Parse(operation.ClassifierID);

						Console.WriteLine("Operation complex classifier. ID=" + classifierID);

						EAAPI.Element classifierElement = operation.GetClassifier(_repository);

						string cacheKey = GetCacheKeyForElement(classifierElement);

						if (!_resources.ContainsKey(cacheKey))
						{
							Resource classifierResource = AddElement(classifierElement);

							Statement classifierStatement = GetClassifierStatement(classifierResource, operationResource);

							if (!_statements.ContainsKey(classifierStatement.ID))
							{
								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
						else
                        {
							Resource classifierResource = _resources[cacheKey];

							Statement classifierStatement = GetClassifierStatement(classifierResource, operationResource);

							if (!_statements.ContainsKey(classifierStatement.ID))
							{
								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
					}
					

					for (short parameterCounter = 0; parameterCounter < operation.Parameters.Count; parameterCounter++)
					{
						EAAPI.Parameter parameterEA = operation.Parameters.GetAt(parameterCounter) as EAAPI.Parameter;

						Resource operationParameterResource = ConvertOperationParameter(parameterEA, eaElement, elementResource.Revision);

						if (!_resources.ContainsKey(operationParameterResource.ID))
						{
							_resources.Add(operationParameterResource.ID, operationParameterResource);
						}

						Statement paramaterConatinmentStatement = GetContainsStatement(operationResource, operationParameterResource);

						if (!_statements.ContainsKey(paramaterConatinmentStatement.ID))
						{
							_statements.Add(paramaterConatinmentStatement.ID, paramaterConatinmentStatement);
						}

						if (parameterEA.ClassifierID == "0" && !string.IsNullOrEmpty(parameterEA.Type))
						{
							Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(parameterEA.Type);

							if (primitiveClassifier != null)
							{

								Statement classifierStatement = GetClassifierStatement(primitiveClassifier, operationParameterResource);
								
								if (!_statements.ContainsKey(classifierStatement.ID))
								{
									_statements.Add(classifierStatement.ID, classifierStatement);
								}
							}
						}
						else if (parameterEA.ClassifierID != "0")
						{
                            EAAPI.Element classifierElement = parameterEA.GetClassifier(_repository);

                            string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);

                            Resource classifierResource = null;

                            if (!_resources.ContainsKey(classifierSpecIfID))
                            {
                                classifierResource = AddElement(classifierElement);
                            }
                            else
                            {
                                classifierResource = _resources[classifierSpecIfID];
                            }

                            Statement classifierStatement = GetClassifierStatement(classifierResource, operationParameterResource);

							if (!_statements.ContainsKey(classifierStatement.ID))
							{
								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
						
					}
				}
			}
		}

		private void AddElementConstraints(EAAPI.Element eaElement, Resource elementResource)
		{
			string connectorSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);

			// connector constraints
			for (short constraintCounter = 0; constraintCounter < eaElement.Constraints.Count; constraintCounter++)
			{
				EAAPI.Constraint elementConstraintEA = eaElement.Constraints.GetAt(constraintCounter) as EAAPI.Constraint;

				if (elementConstraintEA != null)
				{
					Resource connectorConstraint = ConvertElementConstraint(eaElement, elementConstraintEA, constraintCounter + 1);

					if (!_resources.ContainsKey(connectorConstraint.ID))
					{
						_resources.Add(connectorConstraint.ID, connectorConstraint);
					}

					Statement constraintContainsStatement = GetContainsStatementFromSpecIfID(new Key(elementResource.ID, elementResource.Revision),
						new Key(connectorConstraint.ID));

					if (!_statements.ContainsKey(constraintContainsStatement.ID))
					{
						_statements.Add(constraintContainsStatement.ID, constraintContainsStatement);
					}


				}
			}
		}


		private void AddUmlConnectors(EAAPI.Element eaElement)
		{
			for (short counter = 0; counter < eaElement.Connectors.Count; counter++)
			{
				EAAPI.Connector connectorEA = eaElement.Connectors.GetAt(counter) as EAAPI.Connector;

				Statement umlConnector = ConvertUmlConnector(connectorEA);

				if (umlConnector != null)
				{
					if (!_statements.ContainsKey(umlConnector.ID))
					{
						_statements.Add(umlConnector.ID, umlConnector);
					}
				}
				//AddConnectorConstraints(connectorEA, umlConnector);

				//AddConnectorTaggedValues(connectorEA, umlConnector);

			}
		}

		private Resource AddDiagram(EAAPI.Diagram diagram)
		{
			Resource diagramResource = ConvertDiagram(diagram);

			if (!_resources.ContainsKey(diagramResource.ID))
			{
				_resources.Add(diagramResource.ID, diagramResource);
			}

			AddShowsStatements(diagram, diagramResource);

            return diagramResource;
		}

		private void AddShowsStatements(EAAPI.Diagram diagram, Resource diagramResource)
		{
			string diagramSpecIfGUID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

			for (short counter = 0; counter < diagram.DiagramObjects.Count; counter++)
			{
				EAAPI.DiagramObject diagramObject = diagram.DiagramObjects.GetAt(counter) as EAAPI.DiagramObject;

				EAAPI.Element elementOnDiagram = _repository.GetElementByID(diagramObject.ElementID);

				if (elementOnDiagram != null)
				{
					string elementSpecIfGuid = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(elementOnDiagram.ElementGUID);
					string elementSpecIfRevision = EaDateToRevisionConverter.ConvertDateToRevision(elementOnDiagram.Modified);


					Statement statement = GetShowsStatementFromSpecIfID(new Key(diagramResource.ID, diagramResource.Revision), 
																		new Key(elementSpecIfGuid, elementSpecIfRevision));

					if (!_statements.ContainsKey(statement.ID))
					{
						_statements.Add(statement.ID, statement);
					}

					AddElement(elementOnDiagram);
				}
			}

			//for (short counter = 0; counter < diagram.DiagramLinks.Count; counter++)
			//{
			//	EAAPI.DiagramLink link = diagram.DiagramLinks.GetAt(counter) as EAAPI.DiagramLink;

			//	EAAPI.Connector connectorOnDiagram = _repository.GetConnectorByID(link.ConnectorID);

			//	if (connectorOnDiagram != null)
			//	{
			//		string connectorSpecIfGuid = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorOnDiagram.ConnectorGUID);

			//		Statement statement = GetShowsStatementFromSpecIfID(new Key(diagramResource.ID, diagramResource.Revision), new Key(connectorSpecIfGuid));
			//		_statements.Add(statement.ID, statement);
			//	}
			//}
		}

		private void AddConnectorConstraints(EAAPI.Connector connectorEA, Statement connectorStatement)
		{
			string connectorSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorEA.ConnectorGUID);

			// connector constraints
			for (short constraintCounter = 0; constraintCounter < connectorEA.Constraints.Count; constraintCounter++)
			{
				EAAPI.ConnectorConstraint connectorConstraintEA = connectorEA.Constraints.GetAt(constraintCounter) as EAAPI.ConnectorConstraint;

				if (connectorConstraintEA != null)
				{
					Resource connectorConstraint = ConvertConnectorConstraint(connectorEA, connectorConstraintEA, constraintCounter + 1);

					if (!_resources.ContainsKey(connectorConstraint.ID))
					{
						_resources.Add(connectorConstraint.ID, connectorConstraint);
					}

					Statement constraintContainsStatement = GetContainsStatementFromSpecIfID(new Key(connectorStatement.ID, connectorStatement.Revision),
																							 new Key(connectorConstraint.ID));

					if (!_statements.ContainsKey(constraintContainsStatement.ID))
					{
						_statements.Add(constraintContainsStatement.ID, constraintContainsStatement);
					}


				}
			}

			// guard
			if (!string.IsNullOrEmpty(connectorEA.TransitionGuard))
			{

				Resource guardResource = ConvertConnectorGuard(connectorEA);

				if (!_resources.ContainsKey(guardResource.ID))
				{
					_resources.Add(guardResource.ID, guardResource);
				}

				Statement guardContainsStatement = GetContainsStatementFromSpecIfID(new Key(connectorStatement.ID, connectorStatement.Revision),
																					new Key(guardResource.ID));

				if (!_statements.ContainsKey(guardContainsStatement.ID))
				{
					_statements.Add(guardContainsStatement.ID, guardContainsStatement);
				}
			}


		}

		private void AddConnectorTaggedValues(EAAPI.Connector eaConnector, Statement connectorStatement)
		{
			List<Resource> tagResources = ConvertConnectorTaggedValues(eaConnector);

			foreach (Resource tagResource in tagResources)
			{
				if (!_resources.ContainsKey(tagResource.ID))
				{
					_resources.Add(tagResource.ID, tagResource);
				}

				Statement tagContainsStatement = GetContainsStatement(connectorStatement,
																	  tagResource);

				if (!_statements.ContainsKey(tagContainsStatement.ID))
				{
					_statements.Add(tagContainsStatement.ID, tagContainsStatement);
				}
			}
		}

		#endregion

		#region CONVERTERS

		private Resource ConvertOperation(EAAPI.Method operation, EAAPI.Element eaElement, string elementRevision)
		{

			Resource result;

			string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID);

			if (!_resources.ContainsKey(specIfID))
			{

				Resource operationResource = new Resource()
				{
					ID = specIfID,
					Class = new Key("RC-Actor", "1"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = elementRevision,
					Title = new Value(operation.Name)
				};

				operationResource.Properties = new List<Property>();

				operationResource.Properties.Add(
					new Property("dcterms:title", new Key("PC-Name", "1"), operation.Name,
								 EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_NAME"),
								 eaElement.Modified, eaElement.Author));


				operationResource.Properties.Add(
					new Property("dcterms:description", new Key("PC-Description", "1"), operation.Notes,
								 EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_NOTES"),
								 eaElement.Modified, eaElement.Author));


				operationResource.Properties.Add(
					new Property("dcterms:type", new Key("PC-Type", "1"), "OMG:UML:2.5.1:Operation",
								 EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_TYPE"),
								 eaElement.Modified, eaElement.Author));


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

				operationResource.Properties.Add(
						new Property("UML:Stereotype", new Key("PC-Stereotype", "1"), stereotypeValue,
									 EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_STEREOTYPE"),
									 eaElement.Modified, eaElement.Author));


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
					Class = new Key("RC-State", "1"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = elementRevision,
					Title = new Value(parameter.Name)
				};

				result.Properties = new List<Property>();

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", "1"),
						Value = parameter.Name,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_NAME"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Description", "1"),
						Value = parameter.Notes,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_NOTES"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", "1"),
						Value = "OMG:UML:2.5.1:Parameter",
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_TYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);
			}
			else
            {
				result = _resources[specIfID];
            }

			return result;
		}

		public Resource ConvertElement(EAAPI.Element eaElement)
		{
			Resource result;

			string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);

			string cacheKey = GetCacheKeyForElement(eaElement);

			if (!_resources.ContainsKey(cacheKey))
			{

				string elementType = eaElement.Type;

				string revision = EaDateToRevisionConverter.ConvertDateToRevision(eaElement.Modified);

				ResourceClass resourceClass = GetResourceClassForElementType(elementType, eaElement.Stereotype);

				Resource elementResource = new Resource()
				{
					ID = specIfID,
					Class = new Key(resourceClass.ID, resourceClass.Revision),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = revision,
					Title = new Value(eaElement.Name)
				};

				elementResource.Properties = new List<Property>();

				elementResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", "1"),
						Value = eaElement.Name,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NAME"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				elementResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Description", "1"),
						Value = eaElement.Notes,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NOTES"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				if (resourceClass.ID != "RC-Hierarchy")
				{
					elementResource.Properties.Add(
						new Property()
						{
							Title = new Value("SpecIF:Status"),
							PropertyClass = new Key("PC-Status", "1"),
							Value = new Value(GetStatusEnumID(eaElement.Status)),
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_STATUS"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
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
							Title = new Value("UML:Stereotype"),
							PropertyClass = new Key("PC-Stereotype", "1"),
							Value = stereotypeValue,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_STEREOTYPE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
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

						if (elementType == "Object" && stereotype == "agent")
						{
							elementLegacyType = "FMC4SE:Agent";
						}
						if (elementType == "Port" && stereotype == "channel")
						{
							elementLegacyType = "FMC4SE:Channel";
						}
					}

					elementResource.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:type"),
							PropertyClass = new Key("PC-Type", "1"),
							Value = elementLegacyType,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_TYPE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
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
							new Property("dcterms:identifier", new Key("PC-VisibleId", "1"), identifierValue,
										 EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_IDENTIFIER"),
										 eaElement.Modified, eaElement.Author)
							);
					}
				}


				result = elementResource;
			}
			else
            {
				result = _resources[cacheKey];
            }

			return result;
		}

		private List<Resource> ConvertElementTaggedValues(EAAPI.Element eaElement)
		{
			List<Resource> result = new List<Resource>();


			string namespc = "";
			string stereotype = "";

			ParseFullQualifiedName(eaElement.FQStereotype, out namespc, out stereotype);

			for (short counter = 0; counter < eaElement.TaggedValues.Count; counter++)
			{

				EAAPI.TaggedValue tag = eaElement.TaggedValues.GetAt(counter) as EAAPI.TaggedValue;

				string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID);

				if (!_resources.ContainsKey(specIfID))
				{
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
						Class = new Key("RC-State", "1"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author,
						Revision = "1",
						Title = new Value(tagName)
					};

					tagResource.Properties = new List<Property>();

					tagResource.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:title"),
							PropertyClass = new Key("PC-Name", "1"),
							Value = tagName,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_NAME"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
						}
						);

					tagResource.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:description"),
							PropertyClass = new Key("PC-Description", "1"),
							Value = tag.Notes,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_NOTES"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
						}
						);

					tagResource.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:type"),
							PropertyClass = new Key("PC-Type", "1"),
							Value = "OMG:UML:2.5.1:TaggedValue",
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_TYPE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
						}
						);

					tagResource.Properties.Add(
						new Property()
						{
							Title = new Value("rdf:value"),
							PropertyClass = new Key("PC-Value", "1"),
							Value = eaElement.GetTaggedValueString(tag.Name),
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_VALUE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
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
							Title = new Value("UML:Stereotype"),
							PropertyClass = new Key("PC-Stereotype", "1"),
							Value = stereotypeValue,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_STEREOTYPE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
						}
						);

					result.Add(tagResource);
				}
				else
                {
					result.Add(_resources[specIfID]);
                }
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
					Class = new Key("RC-State", "1"),
					Revision = "1",
					Title = new Value(elementConstraint.Name)
				};

				result.Properties = new List<Property>();

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", "1"),
						Value = elementConstraint.Name,
						ID = result.ID + "_NAME"

					}
					);

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", "1"),
						Value = "OMG:UML:2.5.1:Constraint",
						ID = result.ID + "_TYPE"

					}
					);

				result.Properties.Add(
						new Property()
						{
							Title = new Value("UML:Stereotype"),
							PropertyClass = new Key("PC-Stereotype", "1"),
							Value = elementConstraint.Type,
							ID = result.ID + "_STEREOTYPE"
						}
						);

				result.Properties.Add(
						new Property()
						{
							Title = new Value("rdf:value"),
							PropertyClass = new Key("PC-Value", "1"),
							Value = elementConstraint.Notes,
							ID = result.ID + "_VALUE"
						}
						);
			}
			else
            {
				result = _resources[specIfID];
            }

			return result;
		}

		public Resource ConvertDiagram(EAAPI.Diagram diagram)
		{
			Resource result = null;

			string diagramXHTML = "";

			if (diagram != null)
			{

				string specIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

				if (!_resources.ContainsKey(specIfID))
				{


					try
					{

						string diagramFileName = System.IO.Path.GetTempPath() + "/" + diagram.DiagramGUID + ".png";

						_repository.GetProjectInterface().PutDiagramImageToFile(diagram.DiagramGUID, diagramFileName, 1);

						Image image = Image.FromFile(diagramFileName);

						using (MemoryStream m = new MemoryStream())
						{
							image.Save(m, image.RawFormat);
							byte[] imageBytes = m.ToArray();

							// Convert byte[] to Base64 String
							string base64String = Convert.ToBase64String(imageBytes);

							diagramXHTML += "<p><img style=\"max-width:100%\" src=\"data:image/png;base64," + base64String + "\" /></p>";

						}
					}
					catch (Exception exception)
					{

					}

					#region SVG
					//            EaSvgMetadataProvider eaSvgMetadataProvider = new EaSvgMetadataProvider();

					//DiagramToSvgConverter diagramToSvgConverter = new DiagramToSvgConverter(_repository, eaSvgMetadataProvider);

					//            ScalableVectorGraphics svgDiagram = diagramToSvgConverter.ConvertDiagramToSVG(diagram);

					//XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
					//namespaces.Add("di", "http://www.omg.org/spec/DD/20100524/DI");
					//namespaces.Add("dc", "http://www.omg.org/spec/DD/20100524/DC");
					//namespaces.Add("specif", "https://specif.de/schema/v1.0/DI");

					//// Create a Type array.
					//Type[] extraTypes = new Type[2];
					//extraTypes[0] = typeof(Shape);
					//extraTypes[1] = typeof(Edge);
					////extraTypes[2] = typeof(MetadataContent);
					////extraTypes[0] = typeof(SpecIfDiagramInterchangeBase);

					//XmlSerializer xmlSerializer = new XmlSerializer(typeof(ScalableVectorGraphics), extraTypes);

					//using (var stringWriter = new StringWriter())
					//{
					//	using (XmlWriter writer = XmlWriter.Create(stringWriter,
					//		new XmlWriterSettings() {
					//			OmitXmlDeclaration = true,
					//			ConformanceLevel = ConformanceLevel.Auto,
					//			Indent = false
					//		}))
					//	{

					//		xmlSerializer.Serialize(writer, svgDiagram, namespaces);
					//		diagramXHTML = stringWriter.ToString();

					//		//string svg = "<svg><circle cx=\"50\" cy=\"50\" r=\"40\" stroke=\"black\" stroke-width=\"3\" fill=\"red\" /></svg>";

					//		//diagramXHTML = "<p>" + svg + "hallo SVG?</p>";



					//		//System.IO.File.WriteAllText("d:/test/svg/svgx.svg", diagramXHTML);
					//	}
					//}

					#endregion

					string diagramRevision = EaDateToRevisionConverter.ConvertDateToRevision(diagram.ModifiedDate);

					result = new Resource()
					{
						ID = specIfID,
						Class = new Key("RC-Diagram", "1"),
						ChangedAt = diagram.ModifiedDate,
						ChangedBy = diagram.Author,
						Revision = diagramRevision,
						Title = new Value(diagram.Name)
					};

					result.Properties = new List<Property>();

					result.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:title"),
							PropertyClass = new Key("PC-Name", "1"),
							Value = diagram.Name,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_NAME"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:description"),
							PropertyClass = new Key("PC-Description", "1"),
							Value = diagram.Notes,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_NOTES"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("SpecIF:Diagram"),
							PropertyClass = new Key("PC-Diagram", "1"),
							Value = diagramXHTML,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_DIAGRAM"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:type"),
							PropertyClass = new Key("PC-Type", "1"),
							Value = "OMG:UML:2.5.1:" + GetUmlDiagramTypeFromEaType(diagram.Type),
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_TYPE"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

				}
				else
                {
					result = _resources[specIfID];
                }
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
					Class = new Key("RC-State", "1"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = elementRevision,
					Title = new Value(attribute.Name)
				};

				result.Properties = new List<Property>();

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", "1"),
						Value = attribute.Name,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_NAME"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Description", "1"),
						Value = attribute.Notes,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_NOTES"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", "1"),
						Value = "OMG:UML:2.5.1:Attribute",
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_TYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Properties.Add(
						new Property()
						{
							Title = new Value("rdf:value"),
							PropertyClass = new Key("PC-Value", "1"),
							Value = attribute.Default,
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_VALUE"),
							ChangedAt = eaElement.Modified,
							ChangedBy = eaElement.Author
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

		private Statement ConvertUmlConnector(EAAPI.Connector connectorEA)
		{
			Statement result = null;

			EAAPI.Element sourceElement = _repository.GetElementByID(connectorEA.ClientID);
			EAAPI.Element targetElement = _repository.GetElementByID(connectorEA.SupplierID);
						
			Resource sourceResource = AddElement(sourceElement);
			Resource targetResource = AddElement(targetElement);

			if (connectorEA.Type == "Dependency" && connectorEA.Stereotype == "satisfy")
            {
				result = GetSatisfiesStatement(sourceResource, targetResource);
            }

			return result;
		}

		private Resource ConvertConnectorConstraint(EAAPI.Connector connector, EAAPI.ConnectorConstraint connectorConstraint, int index)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connector.ConnectorGUID + "_CONSTRAINT_" + index),
				Class = new Key("RC-State", "1"),
				Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
				Title = new Value(connectorConstraint.Name)
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", "1"),
					Value = connectorConstraint.Name,
					ID = result.ID + "_NAME"
					
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", "1"),
					Value = "OMG:UML:2.5.1:Constraint",
					ID = result.ID + "_TYPE"
					
				}
				);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("UML:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", "1"),
						Value = connectorConstraint.Type,
						ID = result.ID + "_STEREOTYPE"
					}
					);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", "1"),
						Value = connectorConstraint.Notes,
						ID = result.ID + "_VALUE"
					}
					);

			return result;
		}

		private Resource ConvertConnectorGuard(EAAPI.Connector connector)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connector.ConnectorGUID + "_GUARD"),
				Class = new Key("RC-Actor", "1"),
				Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
				Title = new Value(connector.TransitionGuard)
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", "1"),
					Value = connector.TransitionGuard,
					ID = result.ID + "_NAME"

				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", "1"),
					Value = "OMG:UML:2.5.1:Constraint",
					ID = result.ID + "_TYPE"

				}
				);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("UML:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", "1"),
						Value = "Guard",
						ID = result.ID + "_STEREOTYPE"
					}
					);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", "1"),
						Value = connector.TransitionGuard,
						ID = result.ID + "_VALUE"
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
					Class = new Key("RC-State", "1"),
					Revision = SpecIfGuidGenerator.CreateNewSpecIfGUID(),
					Title = new Value(tagName)
				};

				tagResource.Properties = new List<Property>();

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", "1"),
						Value = tagName,
                        ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_NAME")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Description", "1"),
						Value = tag.Notes,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_NOTES")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", "1"),
						Value = "OMG:UML:2.5.1:TaggedValue",
                        ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_TYPE")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", "1"),
						Value = tag.Value,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_VALUE")
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
						Title = new Value("UML:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", "1"),
						Value = stereotypeValue,
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_STEREOTYPE")
					}
					);

				result.Add(tagResource);
			}


			return result;
		}

        private Statement GetClassifierStatement(Resource classifier, Resource classifiedResource)
        {
            Statement result = null;

			string statementID = classifiedResource.ID + "_INSTANCEOF_" + classifier.ID;

			// rdf:type statement
			result = new Statement()
            {
				ID = statementID,
				Revision = "1",
                Title = new Value("rdf:type"),
                Class = new Key("SC-Classifier", "1"),
                StatementSubject = new Key(classifiedResource.ID, classifiedResource.Revision),
                StatementObject = new Key(classifier.ID, classifier.Revision)
            };


            return result;
        }

		private Statement GetSatisfiesStatement(Resource subjectResource, Resource objectResource)
		{
			Statement result;

			string statementID = subjectResource.ID + "_SATISFIES_" + objectResource.ID;

			// rdf:type statement
			result = new Statement()
			{
				ID = statementID,
				Revision = "1",
				Title = new Value("oslc_rm:satisfies"),
				Class = new Key("SC-satisfies", "1"),
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
				Title = new Value("SpecIF:contains"),
				Class = new Key("SC-contains", "1"),
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
				Title = new Value("SpecIF:shows"),
				Class = new Key("SC-shows", "1"),
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
					new Key(subelementSpecIfID, "1")
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
					new Key(subElementSpecIfID, "1")
					);

				result.Add(attributeReferenceStatement);

			} // for

			return result;
		}

		#endregion

		private void ParseFullQualifiedName(string fullQualifiedName, out string namespc, out string name)
		{
			if(fullQualifiedName.Contains("::"))
			{
				string[] separator = { "::" };
				
				string[] tokens = fullQualifiedName.Split(separator, StringSplitOptions.RemoveEmptyEntries);

				if(tokens.Length == 2)
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
				ID = ID,
				Revision = revision,
				PropertyClass = new Key("PC-UML_VisibilityKind", revision),
				Title = new Value("UML:VisibilityKind"),
				Value = new Value()
			};

			if(changedAt != default(DateTime))
			{
				result.ChangedAt = changedAt;
			}

			if(!string.IsNullOrEmpty(changedBy))
			{
				result.ChangedBy = changedBy;
			}

			LanguageValue languageValue = new LanguageValue()
			{
				Text = "V-UML_VisibilityKind-0",
				Language = "en-EN"
			};

			switch (visibility.ToLower())
			{
				case "public":
					languageValue.Text = "V-UML_VisibilityKind-0";					
				break;

				case "private":
					languageValue.Text = "V-UML_VisibilityKind-1";
				break;

				case "protected":
					languageValue.Text = "V-UML_VisibilityKind-2";
				break;

				case "package":
					languageValue.Text = "V-UML_VisibilityKind-3";
				break;

				case "internal":
					languageValue.Text = "V-UML_VisibilityKind-4";
				break;

				case "protected internal":
					languageValue.Text = "V-UML_VisibilityKind-5";
				break;
			}

			result.Value = languageValue.Text;

			return result;
		}

		private ResourceClass GetResourceClassForElementType(string type, string stereotype = "")
		{
			ResourceClass result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1"));

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

					result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1"));
				break;

                case "Object":
                    result = _metadataReader.GetResourceClassByKey(new Key("RC-State", "1"));

                    if (stereotype == "heading")
                    {
                        result = _metadataReader.GetResourceClassByKey(new Key("RC-Folder", "1"));
                    }
					else if(stereotype == "agent")
                    {
						result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1"));
					}
                break;


				case "Port":
					result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1"));
					break;
				
				case "Activity":
				case "Action":
				case "Actor":
                case "StateMachine":
				case "UseCase":
				case "Decision":

					result = _metadataReader.GetResourceClassByKey(new Key("RC-Actor", "1"));
				break;

				case "Package":

                    result = _metadataReader.GetResourceClassByKey(new Key("RC-Collection", "1"));

					if(stereotype == "specification")
                    {
						result = _metadataReader.GetResourceClassByKey(new Key("RC-Hierarchy", "1"));
					}
				break;

				case "Requirement":
					result = _metadataReader.GetResourceClassByKey(new Key("RC-Requirement", "1"));
				break;

				case "StateNode":
				case "Event":
					result = _metadataReader.GetResourceClassByKey(new Key("RC-Event", "1"));
				break;
			}

			return result;
		}

		private string GetUmlDiagramTypeFromEaType(string eaType)
		{
			string result = "";

			switch(eaType)
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

			switch(connectorDirection)
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
				Title = new Value(title),
				Class = new Key(GetResourceClassForElementType("PrimitiveType").ID, "1"),
				ChangedAt = new DateTime(2019, 4, 7),
				ChangedBy = "oalt"

			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", "1"),
					Value = title,
    				ID = result.ID + "_NAME",
					ChangedAt = new DateTime(2019, 4, 7),
					ChangedBy = "oalt"
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:description"),
					PropertyClass = new Key("PC-Description", "1"),
					Value = description,
					ID = result.ID + "_NOTES",
					ChangedAt = new DateTime(2019, 4, 7),
					ChangedBy = "oalt"
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", "1"),
					Value = "OMG:UML:2.5.1:PrimitiveType",
					ID = result.ID + "_TYPE",
					ChangedAt = new DateTime(2019, 4, 7),
					ChangedBy = "oalt"
				}
				);

			return result;

		}

		private Resource GetPrimitiveTypeBySwTypeName(string swTypeName)
		{
			Resource result = null;

			switch(swTypeName)
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
