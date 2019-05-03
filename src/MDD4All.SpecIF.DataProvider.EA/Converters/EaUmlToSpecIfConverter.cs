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

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
	// TODO: attribute stereotypes, run states, behavior references

	public class EaUmlToSpecIfConverter
	{
		private EAAPI.Repository _repository;

		private Dictionary<string, Resource> _primitiveTypes = new Dictionary<string, Resource>();

		private Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
		private Dictionary<string, Statement> _statements = new Dictionary<string, Statement>();
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="repository">The EA repository.</param>
		public EaUmlToSpecIfConverter(EAAPI.Repository repository)
		{
			_repository = repository;

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

			GetModelHierarchyRecursively(selectedPackage, node);

			result.Hierarchies.Add(node);

			foreach (KeyValuePair<string, Resource> keyValuePair in _primitiveTypes)
			{
				result.Resources.Add(keyValuePair.Value);
			}

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

		public Resource GetResourceByID(string id)
		{
			Resource result = null;

			if(_resources.ContainsKey(id))
			{
				result = _resources[id];
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

		private void GetModelHierarchyRecursively(EAAPI.Package currentPackage, Node currentNode)
		{
			Console.WriteLine("Package: " + currentPackage.Name);

			string packageID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentPackage.Element.ElementGUID);

			currentNode.ResourceReference = new Key(packageID, 1);
			currentNode.Description = new Value("Package: " + currentPackage.Name);

			AddElement(currentPackage.Element);

			// recursive call for child packages
			for(short packageCounter = 0; packageCounter < currentPackage.Packages.Count; packageCounter++)
			{
				EAAPI.Package childPackage = currentPackage.Packages.GetAt(packageCounter) as EAAPI.Package;

				Console.WriteLine("Recursive call: " + childPackage.Name);

				
				Statement containsStatement = GetContainsStatement(currentPackage.Element.ElementGUID, childPackage.Element.ElementGUID);
				_statements.Add(containsStatement.ID, containsStatement);

				string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(childPackage.Element.ElementGUID);

				Node childNode = new Node()
				{
				};
				currentNode.Nodes.Add(childNode);

				GetModelHierarchyRecursively(childPackage, childNode);
				
			}

			// diagrams
			for(short diagramCounter = 0; diagramCounter < currentPackage.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentPackage.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

				AddDiagram(diagram);
				Statement containsStatement = GetContainsStatement(currentPackage.Element.ElementGUID, diagram.DiagramGUID);
				_statements.Add(containsStatement.ID, containsStatement);

				string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

				Node node = new Node()
				{
					ResourceReference = new Key(specIfElementID, 1),
					Description = new Value("Diagram: " + diagram.Name)
				};

				currentNode.Nodes.Add(node);
			}

			// elements
			for (short elementCounter = 0; elementCounter < currentPackage.Elements.Count; elementCounter++)
			{
				EAAPI.Element element = currentPackage.Elements.GetAt(elementCounter) as EAAPI.Element;

				Console.WriteLine("Element: " + element.Name);

				AddElement(element);
				Statement containsStatement = GetContainsStatement(currentPackage.Element.ElementGUID, element.ElementGUID);
				_statements.Add(containsStatement.ID, containsStatement);

				GetElementHierarchyRecursively(element, currentNode);
			}
		}

		private void GetElementHierarchyRecursively(EAAPI.Element currentElement, Node currentNode)
		{
			string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentElement.ElementGUID);

			Node node = new Node()
			{
				ResourceReference = new Key(specIfElementID, 1),
				Description = new Value("Element: " + currentElement.Name)
			};

			currentNode.Nodes.Add(node);

			// diagrams
			for (short diagramCounter = 0; diagramCounter < currentElement.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentElement.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

				AddDiagram(diagram);
				Statement containsStatement = GetContainsStatement(currentElement.ElementGUID, diagram.DiagramGUID);
				_statements.Add(containsStatement.ID, containsStatement);

				string specIfDiagramID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

				Node diagramNode = new Node()
				{
					ResourceReference = new Key(specIfDiagramID, 1)
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
					AddElement(subElement);
					Statement containsStatement = GetContainsStatement(currentElement.ElementGUID, subElement.ElementGUID);
					_statements.Add(containsStatement.ID, containsStatement);

					// recursive call
					GetElementHierarchyRecursively(subElement, node);
				}
				
			}

			// embedded elements
			for (short embeddedElementCounter = 0; embeddedElementCounter < currentElement.EmbeddedElements.Count; embeddedElementCounter++)
			{
				EAAPI.Element embeddedElement = currentElement.EmbeddedElements.GetAt(embeddedElementCounter) as EAAPI.Element;

				Console.WriteLine("Embedded element: " + embeddedElement.Name);

				AddElement(embeddedElement);
				Statement containsStatement = GetContainsStatement(currentElement.ElementGUID, embeddedElement.ElementGUID);
				_statements.Add(containsStatement.ID, containsStatement);

				string specIfEmbeddedElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID);

				Node embeddedElementNode = new Node()
				{
					ResourceReference = new Key(specIfEmbeddedElementID, 1),
					Description = new Value("Embedded element: " + embeddedElement.Name)
				};

				node.Nodes.Add(embeddedElementNode);
			}

			
		}

		#region DATA_ADDITION

		private void AddElement(EAAPI.Element eaElement)
		{

			Resource elementResource = ConvertElement(eaElement);

			if(_resources.ContainsKey(elementResource.ID))
			{
				Console.WriteLine("Element still in collection " + elementResource.ID);
			}
			_resources.Add(elementResource.ID, elementResource);

			AddClassifier(eaElement);

			AddTaggedValues(eaElement);

			AddAttributes(eaElement);

			AddOperations(eaElement);

			AddElementConstraints(eaElement);

			AddUmlConnectors(eaElement);

		}

		private void AddClassifier(EAAPI.Element element)
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

				if(!_resources.ContainsKey(classifierSpecIfID))
				{
					AddElement(classifierElement);
				}

				classifierStatement = GetClassifierStatement(classifierID, new Key(specIfID, 1));

				if (classifierStatement != null)
				{
					_statements.Add(classifierStatement.ID, classifierStatement);
				}
			}
			
		}

		private void AddTaggedValues(EAAPI.Element eaElement)
		{
			List<Resource> tagResources = ConvertElementTaggedValues(eaElement);

			foreach (Resource tagResource in tagResources)
			{
				_resources.Add(tagResource.ID, tagResource);
			}

			foreach (Resource tagResource in tagResources)
			{
				Statement tagContainsStatement = GetContainsStatement(eaElement.ElementGUID, EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(tagResource.ID));
				_statements.Add(tagContainsStatement.ID, tagContainsStatement);
			}
		}

		private void AddAttributes(EAAPI.Element eaElement)
		{
			for(short counter = 0; counter < eaElement.Attributes.Count; counter++ )
			{
				EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

				if(attribute != null)
				{
					Resource attributeResource = ConvertAttribute(attribute, eaElement);
					_resources.Add(attributeResource.ID, attributeResource);

					// add contains statement
					Statement containsStatement = GetContainsStatement(eaElement.ElementGUID, attribute.AttributeGUID);

					_statements.Add(containsStatement.ID, containsStatement);

					if (attribute.ClassifierID == 0 && !string.IsNullOrEmpty(attribute.Type))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(attribute.Type);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = new Statement()
							{
								Title = new Value("rdf:type"),
								Class = new Key("SC-Classifier", 1),
								StatementSubject = new Key(attributeResource.ID, attributeResource.Revision),
								StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
							};

							_statements.Add(classifierStatement.ID, classifierStatement);
						}
					}
					else if (attribute.ClassifierID != 0)
					{
						Statement classifierStatement = GetClassifierStatement(attribute.ClassifierID, new Key(attributeResource.ID, attributeResource.Revision));

						_statements.Add(classifierStatement.ID, classifierStatement);
					}
					
				}
			}

		}

		private void AddOperations(EAAPI.Element eaElement)
		{
			for (short counter = 0; counter < eaElement.Methods.Count; counter++)
			{
				EAAPI.Method operation = eaElement.Methods.GetAt(counter) as EAAPI.Method;

				if (operation != null)
				{
					Resource operationResource = ConvertOperation(operation, eaElement);
					_resources.Add(operationResource.ID, operationResource);

					Statement containsStatement = GetContainsStatement(eaElement.ElementGUID, operation.MethodGUID);
					_statements.Add(containsStatement.ID, containsStatement);

					Console.WriteLine("Operation " + operation.Name + " classifierID=" + operation.ClassifierID);

					if (operation.ClassifierID == "0" && !string.IsNullOrEmpty(operation.ReturnType))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(operation.ReturnType);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = new Statement()
							{
								Title = new Value("rdf:type"),
								Class = new Key("SC-Classifier", 1),
								StatementSubject = new Key(operationResource.ID, operationResource.Revision),
								StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
							};

							_statements.Add(classifierStatement.ID, classifierStatement);
						}
					}
					else if (operation.ClassifierID != "0")
					{
						int classifierID = int.Parse(operation.ClassifierID);

						Console.WriteLine("Operation complex classifier. ID=" + classifierID);

						Statement classifierStatement = GetClassifierStatement(classifierID, new Key(operationResource.ID, operationResource.Revision));

						_statements.Add(classifierStatement.ID, classifierStatement);
					}
					

					for (short parameterCounter = 0; parameterCounter < operation.Parameters.Count; parameterCounter++)
					{
						EAAPI.Parameter parameterEA = operation.Parameters.GetAt(parameterCounter) as EAAPI.Parameter;

						Resource operationParameterResource = ConvertOperationParameter(parameterEA, eaElement);

						_resources.Add(operationParameterResource.ID, operationParameterResource);

						Statement paramaterConatinmentStatement = GetContainsStatement(operation.MethodGUID, parameterEA.ParameterGUID);
						_statements.Add(paramaterConatinmentStatement.ID, paramaterConatinmentStatement);

						if (parameterEA.ClassifierID == "0" && !string.IsNullOrEmpty(parameterEA.Type))
						{
							Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(parameterEA.Type);

							if (primitiveClassifier != null)
							{
								Statement classifierStatement = new Statement()
								{
									Title = new Value("rdf:type"),
									Class = new Key("SC-Classifier", 1),
									StatementSubject = new Key(operationParameterResource.ID, operationParameterResource.Revision),
									StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
								};

								_statements.Add(classifierStatement.ID, classifierStatement);
							}
						}
						else if (parameterEA.ClassifierID != "0")
						{
							int classifierID = int.Parse(parameterEA.ClassifierID);

							Statement classifierStatement = GetClassifierStatement(classifierID, new Key(operationParameterResource.ID, operationParameterResource.Revision));

							_statements.Add(classifierStatement.ID, classifierStatement);
						}
						
					}
				}
			}
		}

		private void AddElementConstraints(EAAPI.Element eaElement)
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

						Statement constraintContainsStatement = GetContainsStatementFromSpecIfID(connectorSpecIfID, connectorConstraint.ID);

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

				if (!_statements.ContainsKey(umlConnector.ID))
				{
					_statements.Add(umlConnector.ID, umlConnector);
				}

				AddConnectorConstraints(connectorEA);

				AddConnectorTaggedValues(connectorEA);

			}
		}

		private void AddDiagram(EAAPI.Diagram diagram)
		{
			Resource diagramResource = ConvertDiagram(diagram);
			_resources.Add(diagramResource.ID, diagramResource);

			AddShowsStatements(diagram);
		}

		private void AddShowsStatements(EAAPI.Diagram diagram)
		{
			string diagramSpecIfGUID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

			for (short counter = 0; counter < diagram.DiagramObjects.Count; counter++)
			{
				EAAPI.DiagramObject diagramObject = diagram.DiagramObjects.GetAt(counter) as EAAPI.DiagramObject;

				EAAPI.Element elementOnDiagram = _repository.GetElementByID(diagramObject.ElementID);

				if (elementOnDiagram != null)
				{
					string elementSpecIfGuid = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(elementOnDiagram.ElementGUID);

					Statement statement = GetShowsStatementFromSpecIfID(diagramSpecIfGUID, elementSpecIfGuid);
					_statements.Add(statement.ID, statement);
				}
			}

			for (short counter = 0; counter < diagram.DiagramLinks.Count; counter++)
			{
				EAAPI.DiagramLink link = diagram.DiagramLinks.GetAt(counter) as EAAPI.DiagramLink;

				EAAPI.Connector connectorOnDiagram = _repository.GetConnectorByID(link.ConnectorID);

				if (connectorOnDiagram != null)
				{
					string connectorSpecIfGuid = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorOnDiagram.ConnectorGUID);

					Statement statement = GetShowsStatementFromSpecIfID(diagramSpecIfGUID, connectorSpecIfGuid);
					_statements.Add(statement.ID, statement);
				}
			}
		}

		private void AddConnectorConstraints(EAAPI.Connector connectorEA)
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

						Statement constraintContainsStatement = GetContainsStatementFromSpecIfID(connectorSpecIfID, connectorConstraint.ID);

						_statements.Add(constraintContainsStatement.ID, constraintContainsStatement);

					}

				}
			}
		}

		private void AddConnectorTaggedValues(EAAPI.Connector eaConnector)
		{
			List<Resource> tagResources = ConvertConnectorTaggedValues(eaConnector);

			foreach (Resource tagResource in tagResources)
			{
				_resources.Add(tagResource.ID, tagResource);
			}

			foreach (Resource tagResource in tagResources)
			{
				Statement tagContainsStatement = GetContainsStatement(eaConnector.ConnectorGUID, 
																	  EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(tagResource.ID));

				_statements.Add(tagContainsStatement.ID, tagContainsStatement);
			}
		}

		#endregion

		#region CONVERTERS

		private Resource ConvertOperation(EAAPI.Method operation, EAAPI.Element eaElement)
		{
			Resource operationResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID),
				Class = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(operation.Name)
			};

			operationResource.Properties = new List<Property>();

			operationResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = operation.Name
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_NAME"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			operationResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:description"),
					PropertyClass = new Key("PC-Text", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = operation.Notes
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_NOTES"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			operationResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:Operation"
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_TYPE"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			string namespc = "";
			string stereotype = "";

			ParseFullQualifiedName(operation.FQStereotype, out namespc, out stereotype);

			string stereotypeValue = "";

			if (namespc != "EAUML" && !string.IsNullOrWhiteSpace(namespc))
			{
				stereotypeValue = namespc + ":";
			}
			else if(namespc == "EAUML" && stereotype != "")
			{
				stereotypeValue = "UML:";
			}

			stereotypeValue += stereotype;
			
			if(operation.ReturnType == "entry")
			{
				if(stereotypeValue != "")
				{
					stereotypeValue += ",";
				}
				stereotypeValue += "entry";
			}
			else if(operation.ReturnType == "exit")
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
					new Property()
					{
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = stereotypeValue
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID + "_STEREOTYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

			string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID) + "_VISIBILITY";

			operationResource.Properties.Add(GetVisibilityProperty(operation.Visibility, id, eaElement.Modified, eaElement.Author));

			return operationResource;
		}

		private Resource ConvertOperationParameter(EAAPI.Parameter parameter, EAAPI.Element eaElement)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID),
				Class = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(parameter.Name)
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = parameter.Name
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_NAME"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:description"),
					PropertyClass = new Key("PC-Text", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = parameter.Notes
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_NOTES"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:Parameter"
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID + "_TYPE"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			return result;
		}

		private Resource ConvertElement(EAAPI.Element eaElement)
		{
			Resource result = null;

			string elementType = eaElement.Type;


			string resourceClass = GetResourceClassForElementType(elementType);

			Resource elementResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID),
				Class = new Key(resourceClass, 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(eaElement.Name)
			};

			elementResource.Properties = new List<Property>();

			elementResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
							new LanguageValue
							{
								Text = eaElement.Name
							}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NAME"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			elementResource.Properties.Add(
				new Property()
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
				}
				);

			if (resourceClass == "RC-UML_PassiveElement" || resourceClass == "RC-UML_ActiveElement" ||
				resourceClass == "RC-UML_Package")
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
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
								new LanguageValue
								{
									Text = stereotypeValue
								}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_STEREOTYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				if (elementType != "Package")
				{
					elementResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
								new LanguageValue
								{
									Text = "OMG:UML:2.5.1:" + eaElement.Type
								}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_TYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

					string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID) + "_VISIBILITY";

					elementResource.Properties.Add(GetVisibilityProperty(eaElement.Visibility, id, eaElement.Modified, eaElement.Author));

				}



			}

			result = elementResource;

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

				string tagNamespace = "";
				string tagStereotype = "";
				string tagName = tag.Name;

				ParseFullQualifiedTagName(tag.FQName, out tagNamespace, out tagStereotype, out tagName);

				Resource tagResource = new Resource()
				{
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID),
					Class = new Key("RC-UML_ActiveElement", 1),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = Key.FIRST_MAIN_REVISION,
					Title = new Value(tagName)
				};

				tagResource.Properties = new List<Property>();

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = tagName
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_NAME"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Text", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = tag.Notes
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_NOTES"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:TaggedValue"
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_TYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = eaElement.GetTaggedValueString(tag.Name)
									}
							}
						},
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
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = stereotypeValue
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID + "_STEREOTYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				result.Add(tagResource);
			}


			return result;
		}

		private Resource ConvertElementConstraint(EAAPI.Element element, EAAPI.Constraint elementConstraint, int index)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID + "_CONSTRAINT_" + index),
				Class = new Key("RC-UML_ActiveElement", 1),
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(elementConstraint.Name)
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = elementConstraint.Name
									}
						}
					},
					ID = result.ID + "_NAME"

				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:Constraint"
									}
						}
					},
					ID = result.ID + "_TYPE"

				}
				);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = elementConstraint.Type
									}
							}
						},
						ID = result.ID + "_STEREOTYPE"
					}
					);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = elementConstraint.Notes
									}
							}
						},
						ID = result.ID + "_VALUE"
					}
					);

			return result;
		}

		public Resource ConvertDiagram(EAAPI.Diagram diagram)
		{
			Resource result = null;

			string diagramXHTML = "";

			if (diagram != null)
			{

				try
				{

					string diagramFileName = Path.GetTempPath() + "/" + diagram.DiagramGUID + ".png";

					_repository.GetProjectInterface().PutDiagramImageToFile(diagram.DiagramGUID, diagramFileName, 1);

					Image image = Image.FromFile(diagramFileName);

					using (MemoryStream m = new MemoryStream())
					{
						image.Save(m, image.RawFormat);
						byte[] imageBytes = m.ToArray();

						// Convert byte[] to Base64 String
						string base64String = Convert.ToBase64String(imageBytes);

						diagramXHTML += "<p><img src=\"data:image/png;base64," + base64String + "\" /></p>";

					}
				}
				catch (Exception exception)
				{

				}



				if (diagram != null)
				{
					result = new Resource()
					{
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID),
						Class = new Key("RC-Diagram", 1),
						ChangedAt = diagram.ModifiedDate,
						ChangedBy = diagram.Author,
						Revision = Key.FIRST_MAIN_REVISION,
						Title = new Value(diagram.Name)
					};

					result.Properties = new List<Property>();

					result.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:title"),
							PropertyClass = new Key("PC-Name", 1),
							Value = new Value
							{
								LanguageValues = new List<LanguageValue>
								{
							new LanguageValue
							{
								Text = diagram.Name
							}
								}
							},
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_NAME"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("dcterms:description"),
							PropertyClass = new Key("PC-Text", 1),
							Value = new Value
							{
								LanguageValues = new List<LanguageValue>
								{
									new LanguageValue
									{
										Text = diagram.Notes
									}
								}
							},
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_NOTES"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("SpecIF:Diagram"),
							PropertyClass = new Key("PC-Diagram", 1),
							Value = new Value
							{
								LanguageValues = new List<LanguageValue>
								{
									new LanguageValue
									{
										Text = diagramXHTML
									}
								}
							},
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_DIAGRAM"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

					result.Properties.Add(
						new Property()
						{
							Title = new Value("SpecIF:Notation"),
							PropertyClass = new Key("PC-Notation", 1),
							Value = new Value
							{
								LanguageValues = new List<LanguageValue>
								{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:" + GetUmlDiagramTypeFromEaType(diagram.Type)
									}
								}
							},
							ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID + "_NOTATION"),
							ChangedAt = diagram.ModifiedDate,
							ChangedBy = diagram.Author
						}
						);

				}
			}

			return result;
		}

		private Resource ConvertAttribute(EAAPI.Attribute attribute, EAAPI.Element eaElement)
		{

			Resource attributeResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID),
				Class = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(attribute.Name)
			};

			attributeResource.Properties = new List<Property>();

			attributeResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = attribute.Name
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_NAME"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			attributeResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:description"),
					PropertyClass = new Key("PC-Text", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = attribute.Notes
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_NOTES"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			attributeResource.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:Attribute"
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_TYPE"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			attributeResource.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = attribute.Default
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID + "_VALUE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

			string id = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID) + "_VISIBILITY";

			attributeResource.Properties.Add(GetVisibilityProperty(attribute.Visibility, id, eaElement.Modified, eaElement.Author));

			return attributeResource;
		}

		private Statement ConvertUmlConnector(EAAPI.Connector connectorEA)
		{
			Statement result;

			EAAPI.Element sourceElement = _repository.GetElementByID(connectorEA.ClientID);
			EAAPI.Element targetElement = _repository.GetElementByID(connectorEA.SupplierID);

			string sourceID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(sourceElement.ElementGUID);
			string targetID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(targetElement.ElementGUID);

			string connectorID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorEA.ConnectorGUID);

			Statement umlRelationship = new Statement()
			{
				ID = connectorID,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value("UML:Relationship"),
				Class = new Key("SC-UML_Relationship"),
				StatementSubject = new Key(sourceID, 1),
				StatementObject = new Key(targetID, 1)
			};

			if (connectorEA.Type == "Dependency" && connectorEA.Stereotype == "satisfy")
			{
				umlRelationship.Title = new Value("oslc_rm:satisfies");
				umlRelationship.Class = new Key("SC-satisfies");
			}
			else
			{

				umlRelationship.Properties = new List<Property>();

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.Name
									}
							}
						},
						ID = connectorID + "_NAME"
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Text", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.Notes
									}
							}
						},
						ID = connectorID + "_NOTES"

					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:" + connectorEA.Type
									}
							}
						},
						ID = connectorID + "_TYPE"
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("UML:ConnectorDirection"),
						PropertyClass = new Key("PC-UML_ConnectorDirection", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = GetDirectionID(connectorEA.Direction)
									}
							}
						},
						ID = connectorID + "_DIRECTION"
					}
					);

				string namespc = "";
				string stereotype = "";

				ParseFullQualifiedName(connectorEA.FQStereotype, out namespc, out stereotype);

				string stereotypeValue = "";

				if (namespc != "")
				{
					stereotypeValue = namespc + ":" + stereotype;
				}
				else
				{
					stereotypeValue = stereotype;
				}

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
								new LanguageValue
								{
									Text = stereotypeValue
								}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorID + "_STEREOTYPE"),
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("UML:ConnectorSourceRole"),
						PropertyClass = new Key("PC-UML_ConnectorSourceRole", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.ClientEnd.Role
									}
							}
						},
						ID = connectorID + "_SOURCE_ROLE"
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("UML:ConnectorTargetRole"),
						PropertyClass = new Key("PC-UML_ConnectorTargetRole", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.SupplierEnd.Role
									}
							}
						},
						ID = connectorID + "_TARGET_ROLE"
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("UML:ConnectorSourceMultiplicity"),
						PropertyClass = new Key("PC-UML_ConnectorSourceMultiplicity", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.ClientEnd.Cardinality                                    }
							}
						},
						ID = connectorID + "_SOURCE_MULTIPLICITY"
					}
					);

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = new Value("UML:ConnectorTargetMultiplicity"),
						PropertyClass = new Key("PC-UML_ConnectorTargetMultiplicity", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.ClientEnd.Cardinality
									}
							}
						},
						ID = connectorID + "_TARGET_MULTIPLICITY"
					}
					);


				string sourceId = connectorID + "_VISIBILITY";

				umlRelationship.Properties.Add(GetVisibilityProperty(connectorEA.ClientEnd.Visibility, sourceId));

				string targetId = connectorID + "_VISIBILITY";

				umlRelationship.Properties.Add(GetVisibilityProperty(connectorEA.SupplierEnd.Visibility, targetId));
			}

			result = umlRelationship;

			return result;
		}

		private Resource ConvertConnectorConstraint(EAAPI.Connector connector, EAAPI.ConnectorConstraint connectorConstraint, int index)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connector.ConnectorGUID + "_CONSTRAINT_" + index),
				Class = new Key("RC-UML_ActiveElement", 1),
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(connectorConstraint.Name)
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = connectorConstraint.Name
									}
						}
					},
					ID = result.ID + "_NAME"
					
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:Constraint"
									}
						}
					},
					ID = result.ID + "_TYPE"
					
				}
				);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorConstraint.Type
									}
							}
						},
						ID = result.ID + "_STEREOTYPE"
					}
					);

			result.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorConstraint.Notes
									}
							}
						},
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
					Class = new Key("RC-UML_ActiveElement", 1),
					Revision = Key.FIRST_MAIN_REVISION,
					Title = new Value(tagName)
				};

				tagResource.Properties = new List<Property>();

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:title"),
						PropertyClass = new Key("PC-Name", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = tagName
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_NAME")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:description"),
						PropertyClass = new Key("PC-Text", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = tag.Notes
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_NOTES")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("dcterms:type"),
						PropertyClass = new Key("PC-Type", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = "OMG:UML:2.5.1:TaggedValue"
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_TYPE")
					}
					);

				tagResource.Properties.Add(
					new Property()
					{
						Title = new Value("rdf:value"),
						PropertyClass = new Key("PC-Value", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = tag.Value
									}
							}
						},
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
						Title = new Value("SpecIF:Stereotype"),
						PropertyClass = new Key("PC-Stereotype", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = stereotypeValue
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.TagGUID + "_STEREOTYPE")
					}
					);

				result.Add(tagResource);
			}


			return result;
		}

		private Statement GetClassifierStatement(int classifierID, Key classifiedResource)
		{
			Statement result = null;

			if (classifierID != 0)
			{
				EAAPI.Element classifierElement = _repository.GetElementByID(classifierID);

				if (classifierElement != null)
				{
					string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierElement.ElementGUID);

					// rdf:type statement
					result = new Statement()
					{
						Title = new Value("rdf:type"),
						Class = new Key("SC-Classifier", 1),
						StatementSubject = new Key(classifiedResource.ID, classifiedResource.Revision),
						StatementObject = new Key(classifierSpecIfID, 1)
					};
				}
			}

			return result;
		}

		private Statement GetContainsStatement(string containerEaID, string containedElementEaID)
		{
			Statement result = null;

			string containerSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(containerEaID);
			string subElementSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(containedElementEaID);

			// SpecIF:contains statement
			result = GetContainsStatementFromSpecIfID(containerSpecIfID, subElementSpecIfID);

			return result;
		}

		private Statement GetContainsStatementFromSpecIfID(string subjectID, string objectID)
		{
			Statement result = null;

			
			// SpecIF:contains statement
			result = new Statement()
			{
				Title = new Value("SpecIF:contains"),
				Class = new Key("SC-contains", 1),
				StatementSubject = new Key(subjectID, 1),
				StatementObject = new Key(objectID, 1)
			};

			return result;
		}

		private Statement GetShowsStatementFromSpecIfID(string subjectID, string objectID)
		{
			Statement result = null;


			// SpecIF:shows statement
			result = new Statement()
			{
				Title = new Value("SpecIF:shows"),
				Class = new Key("SC-shows", 1),
				StatementSubject = new Key(subjectID, 1),
				StatementObject = new Key(objectID, 1)
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
				Statement attributeReferenceStatement = new Statement()
				{
					Title = new Value("SpecIF:contains"),
					Class = new Key("SC-contains"),
					StatementSubject = new Key(elementResource.ID, elementResource.Revision),
					StatementObject = new Key(subelementSpecIfID, 1)
				};

				result.Add(attributeReferenceStatement);

			} // for


			for (short counter = 0; counter < eaElement.EmbeddedElements.Count; counter++)
			{
				EAAPI.Element subElement = eaElement.EmbeddedElements.GetAt(counter) as EAAPI.Element;

				string subElementSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(subElement.ElementGUID);

				// add UML:AtributeReference statement
				Statement attributeReferenceStatement = new Statement()
				{
					Title = new Value("SpecIF:contains"),
					Class = new Key("SC-contains"),
					StatementSubject = new Key(elementResource.ID, elementResource.Revision),
					StatementObject = new Key(subElementSpecIfID, 1)
				};

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
		
		private Property GetVisibilityProperty(string visibility, string ID, DateTime changedAt = default(DateTime), string changedBy = "")
		{
			Property result = new Property()
			{
				ID = ID,
				Revision = Key.FIRST_MAIN_REVISION,
				PropertyClass = new Key("PC-UML_VisibilityKind", 1),
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

			result.Value.LanguageValues.Add(languageValue);

			return result;
		}

		private string GetResourceClassForElementType(string type, string stereotype = "")
		{
			string result = "RC-UML_PassiveElement";

			switch(type)
			{
				case "Class":
				case "Component":
				case "State":
				case "StateMachine":
				case "Stereotype":
				case "Parameter":
				case "Port":
				case "ActionPin":
				case "ActivityParameter":
				case "PrimitiveType":

					result = "RC-UML_PassiveElement";
				break;

				case "Activity":
				case "Action":
				case "Actor":
				case "Event":
				case "TaggedValue":
				case "RunState":
				case "UseCase":

					result = "RC-UML_ActiveElement";
				break;

				case "Object":
					result = "RC-UML_ActiveElement";

					if(stereotype == "heading")
					{
						result = "RC-Folder";
					}
				break;

				case "Package":

					result = "RC-UML_Package";
				break;

				case "Requirement":
					result = "RC-Requirement";
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
			Resource integerResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{239A1D1D-046D-4902-87B1-4D86FB6B7D76}"),
				"Integer",
				"An instance of Integer is a value in the (infinite) set of integers (…-2, -1, 0, 1, 2…).");

			_primitiveTypes.Add("Integer", integerResource);

			Resource booleanResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{26844644-6F52-40A8-BDA8-43A61276B197}"),
				"Boolean",
				"An instance of Boolean is one of the predefined values true and false.");

			_primitiveTypes.Add("Boolean", booleanResource);

			Resource stringResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{4CEEB645-62E8-44B0-A97E-694FF01C25E4}"),
				"String",
				@"An instance of String defines a sequence of characters. Character sets may include non-Roman
alphabets.The semantics of the string itself depends on its purpose; it can be a comment,
computational language expression, OCL expression, etc.");

			_primitiveTypes.Add("String", stringResource);

			Resource unlimitedNaturalResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{C35A5389-2CBF-47FB-B987-A540E71B0BFC}"),
				"UnlimitedNatural",
				@"An instance of UnlimitedNatural is a value in the (infinite) set of natural numbers (0, 1, 2…) plus
unlimited. The value of unlimited is shown using an asterisk (‘*’). UnlimitedNatural values are
typically used to denote the upper bound of a range, such as a multiplicity; unlimited is used
whenever the range is specified to have no upper bound.");

			_primitiveTypes.Add("UnlimitedNatural", unlimitedNaturalResource);

			Resource realResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{ABA6441D-268A-457D-A775-484C7301AF21}"),
				"Real",
				@"An instance of Real is a value in the (infinite) set of real numbers. Typically an implementation
will internally represent Real numbers using a floating point standard such as ISO/IEC/IEEE
60559:2011 (whose content is identical to the predecessor IEEE 754 standard).");

			_primitiveTypes.Add("Real", realResource);
		}

		private Resource CreatePrimitiveTypeResource(string id, 
													 string title,
													 string description)
		{
			Resource result = new Resource()
			{
				ID = id,
				Revision = Key.FIRST_MAIN_REVISION,
				Title = new Value(title),
				Class = new Key(GetResourceClassForElementType("PrimitiveType"), 1),
				ChangedAt = new DateTime(2019, 4, 7),
				ChangedBy = "oalt"

			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:title"),
					PropertyClass = new Key("PC-Name", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
						new LanguageValue
							{
								Text = title
							}
						}
					},
					ID = result.ID + "_NAME",
					ChangedAt = new DateTime(2019, 4, 7),
					ChangedBy = "oalt"
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:description"),
					PropertyClass = new Key("PC-Text", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
							new LanguageValue
							{
								Text = description
							}
						}
					},
					ID = result.ID + "_NOTES",
					ChangedAt = new DateTime(2019, 4, 7),
					ChangedBy = "oalt"
				}
				);

			result.Properties.Add(
				new Property()
				{
					Title = new Value("dcterms:type"),
					PropertyClass = new Key("PC-Type", 1),
					Value = new Value
					{
						LanguageValues = new List<LanguageValue>
						{
							new LanguageValue
							{
								Text = "OMG:UML:2.5.1:PrimitiveType"
							}
						}
					},
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
					result = _primitiveTypes["Integer"];
				break;

				case "bool":
				case "boolean":
					result = _primitiveTypes["Boolean"];
				break;

				case "string":
				case "String":
					result = _primitiveTypes["String"];
				break;

				case "uint":
				case "uword":
					result = _primitiveTypes["UnlimitedNatural"];
				break;

				case "float":
				case "double":
					result = _primitiveTypes["Real"];
				break;
			}

			return result;
		}

	}
}
