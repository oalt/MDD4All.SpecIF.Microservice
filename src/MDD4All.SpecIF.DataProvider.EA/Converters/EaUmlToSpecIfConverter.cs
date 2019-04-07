using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;
using MDD4All.SpecIF.DataModels;
using MDD4All.EnterpriseArchitect.Manipulations;
using System.IO;
using System.Drawing;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
	// TODO: attribute stereotypes, connector visibility, operations, run states

	public class EaUmlToSpecIfConverter
	{
		private EAAPI.Repository _repository;

		private Dictionary<string, Resource> _primitiveTypes = new Dictionary<string, Resource>();
		
		public EaUmlToSpecIfConverter(EAAPI.Repository repository)
		{
			_repository = repository;

			InitializePrimitiveTypes();
		}

		public Node GetModelHierarchy(EAAPI.Package selectedPackage)
		{
			Node result = new Node();

			GetModelHierarchyRecursively(selectedPackage, result);
			
			return result;
		}

		private void GetModelHierarchyRecursively(EAAPI.Package currentPackage, Node currentNode)
		{
			string packageID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentPackage.Element.ElementGUID);

			currentNode.ResourceReference = new Key(packageID, 1);

			// recursive call for child packages
			for(short packageCounter = 0; packageCounter < currentPackage.Packages.Count; packageCounter++)
			{
				EAAPI.Package childPackage = currentPackage.Packages.GetAt(packageCounter) as EAAPI.Package;

				Node childNode = new Node();
				currentNode.Nodes.Add(childNode);

				GetModelHierarchyRecursively(childPackage, childNode);
				Console.WriteLine("Recursive call :" + childPackage.Name);
			}

			// diagrams
			for(short diagramCounter = 0; diagramCounter < currentPackage.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentPackage.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

				string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(diagram.DiagramGUID);

				Node node = new Node()
				{
					ResourceReference = new Key(specIfElementID, 1)
				};

				currentNode.Nodes.Add(node);
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
			string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentElement.ElementGUID);

			Node node = new Node()
			{
				ResourceReference = new Key(specIfElementID, 1)
			};

			currentNode.Nodes.Add(node);

			// diagrams
			for (short diagramCounter = 0; diagramCounter < currentElement.Diagrams.Count; diagramCounter++)
			{
				EAAPI.Diagram diagram = currentElement.Diagrams.GetAt(diagramCounter) as EAAPI.Diagram;

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

				// recursive call
				GetElementHierarchyRecursively(subElement, node);
			}

			// embedded elements
			for (short embeddedElementCounter = 0; embeddedElementCounter < currentElement.EmbeddedElements.Count; embeddedElementCounter++)
			{
				EAAPI.Element embeddedElement = currentElement.EmbeddedElements.GetAt(embeddedElementCounter) as EAAPI.Element;

				string specIfEmbeddedElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID);

				Node embeddedElementNode = new Node()
				{
					ResourceReference = new Key(specIfEmbeddedElementID, 1)
				};

				node.Nodes.Add(embeddedElementNode);
			}

			// Attributes
			//for (short counter = 0; counter < currentElement.Attributes.Count; counter++)
			//{
			//	EAAPI.Attribute attribute = currentElement.Attributes.GetAt(counter) as EAAPI.Attribute;

			//	if (attribute != null)
			//	{
			//		string specIfAttributreID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID);

			//		Node attributeNode = new Node()
			//		{
			//			ResourceReference = new Key(specIfAttributreID, 1)
			//		};

			//		currentNode.Nodes.Add(attributeNode);
			//	}
			//}


			// Operations
		}

		public void AddSpecIfDataBasedOnHierarchy(DataModels.SpecIF specIF)
		{
			foreach(KeyValuePair<string, Resource> keyValuePair in _primitiveTypes)
			{
				specIF.Resources.Add(keyValuePair.Value);
			}

			if (specIF.Hierarchies.Count > 0)
			{
				AddSpecIfDataBasedOnHierarchyRecursively(specIF.Hierarchies[0], specIF);
			}
		}

		private void AddSpecIfDataBasedOnHierarchyRecursively(Node currentNode, DataModels.SpecIF specIF)
		{
			Resource resource = GetUmlElementResource(currentNode.ResourceReference);

			if(resource != null)
			{
				string eaGUID = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(resource.ID);

				EAAPI.Element element = _repository.GetElementByGuid(eaGUID);

				List<Resource> tagResources = ConvertElementTaggedValues(element);

				specIF.Resources.AddRange(tagResources);

				foreach(Resource tagResource in tagResources)
				{
					specIF.Statements.Add(GetContainsStatement(eaGUID, EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(tagResource.ID)));
				}

				List<Resource> attributeResources = AddAttributes(element, specIF);

				//foreach (Resource attributeResource in attributeResources)
				//{
				//	specIF.Statements.Add(GetContainsStatement(eaGUID, EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(attributeResource.ID)));
				//}

				List<Resource> operationResources = AddOperations(element, specIF);

				//foreach (Resource operationResource in operationResources)
				//{
				//	specIF.Statements.Add(GetContainsStatement(eaGUID, EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(operationResource.ID)));
				//}
			}

			if(resource == null) // Is it a diagram GUID?
			{
				resource = GetDiagramResource(currentNode.ResourceReference);
			}			

			if (resource != null)
			{
				specIF.Resources.Add(resource);

				List<Statement> statements = GetAllStatementsForResource(currentNode.ResourceReference);

				foreach (Statement statement in statements)
				{
					if (specIF.Statements.Find(sm => sm.ID == statement.ID) == null) // avoid to add a connection 2 times
					{
						specIF.Statements.Add(statement);
					}
				}
			}

			foreach (Node childNode in currentNode.Nodes)
			{
				specIF.Statements.Add(
					GetContainsStatement(
					EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(currentNode.ResourceReference.ID), 
					EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(childNode.ResourceReference.ID)));

				AddSpecIfDataBasedOnHierarchyRecursively(childNode, specIF);
			}
		}

		private List<Resource> AddAttributes(EAAPI.Element eaElement, DataModels.SpecIF specIF)
		{
			List<Resource> result = new List<Resource>();

			for(short counter = 0; counter < eaElement.Attributes.Count; counter++ )
			{
				EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

				if(attribute != null)
				{
					Resource attributeResource = ConvertAttribute(attribute, eaElement);
					specIF.Resources.Add(attributeResource);
					result.Add(attributeResource);

					if (attribute.ClassifierID == 0 && !string.IsNullOrEmpty(attribute.Type))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(attribute.Type);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = new Statement()
							{
								Title = "rdf:type",
								StatementClass = new Key("SC-Classifier", 1),
								StatementSubject = new Key(attributeResource.ID, attributeResource.Revision),
								StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
							};

							specIF.Statements.Add(classifierStatement);
						}
					}
					else if (attribute.ClassifierID != 0)
					{
						Statement classifierStatement = GetClassifierStatement(attribute.ClassifierID, new Key(attributeResource.ID, attributeResource.Revision));

						specIF.Statements.Add(classifierStatement);
					}
					
				}
			}

			return result;
		}

		private List<Resource> AddOperations(EAAPI.Element eaElement, DataModels.SpecIF specIF)
		{
			List<Resource> result = new List<Resource>();

			for (short counter = 0; counter < eaElement.Methods.Count; counter++)
			{
				EAAPI.Method operation = eaElement.Methods.GetAt(counter) as EAAPI.Method;

				if (operation != null)
				{
					Resource operationResource = ConvertOperation(operation, eaElement);
					specIF.Resources.Add(operationResource);
					result.Add(operationResource);

					Console.WriteLine("Operation " + operation.Name + " classifierID=" + operation.ClassifierID);

					if (operation.ClassifierID == "0" && !string.IsNullOrEmpty(operation.ReturnType))
					{
						Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(operation.ReturnType);

						if (primitiveClassifier != null)
						{
							Statement classifierStatement = new Statement()
							{
								Title = "rdf:type",
								StatementClass = new Key("SC-Classifier", 1),
								StatementSubject = new Key(operationResource.ID, operationResource.Revision),
								StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
							};

							specIF.Statements.Add(classifierStatement);
						}
					}
					else if (operation.ClassifierID != "0")
					{
						int classifierID = int.Parse(operation.ClassifierID);

						Console.WriteLine("Operation complex classifier. ID=" + classifierID);

						Statement classifierStatement = GetClassifierStatement(classifierID, new Key(operationResource.ID, operationResource.Revision));

						specIF.Statements.Add(classifierStatement);
					}
					

					for (short parameterCounter = 0; parameterCounter < operation.Parameters.Count; parameterCounter++)
					{
						EAAPI.Parameter parameterEA = operation.Parameters.GetAt(parameterCounter) as EAAPI.Parameter;

						Resource operationParameterResource = ConvertOperationParameter(parameterEA, eaElement);

						specIF.Resources.Add(operationParameterResource);

						Statement paramaterConatinmentStatement = GetContainsStatement(operation.MethodGUID, parameterEA.ParameterGUID);

						specIF.Statements.Add(paramaterConatinmentStatement);

						if (parameterEA.ClassifierID == "0" && !string.IsNullOrEmpty(parameterEA.Type))
						{
							Resource primitiveClassifier = GetPrimitiveTypeBySwTypeName(parameterEA.Type);

							if (primitiveClassifier != null)
							{
								Statement classifierStatement = new Statement()
								{
									Title = "rdf:type",
									StatementClass = new Key("SC-Classifier", 1),
									StatementSubject = new Key(operationParameterResource.ID, operationParameterResource.Revision),
									StatementObject = new Key(primitiveClassifier.ID, primitiveClassifier.Revision)
								};

								specIF.Statements.Add(classifierStatement);
							}
						}
						else if (parameterEA.ClassifierID != "0")
						{
							int classifierID = int.Parse(parameterEA.ClassifierID);

							Statement classifierStatement = GetClassifierStatement(classifierID, new Key(operationParameterResource.ID, operationParameterResource.Revision));

							specIF.Statements.Add(classifierStatement);
						}
						
					}
				}
			}

			return result;
		}

		private Resource ConvertOperation(EAAPI.Method operation, EAAPI.Element eaElement)
		{
			Resource operationResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(operation.MethodGUID),
				ResourceClass = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = 1,
				Title = operation.Name
			};

			operationResource.Properties = new List<Property>();

			operationResource.Properties.Add(
				new Property()
				{
					Title = "dcterms:title",
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
					Title = "dcterms:description",
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
					Title = "dcterms:type",
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

			string stereotypeValue = "UML:";

			if (namespc != "EAUML")
			{
				stereotypeValue = namespc + ":";
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
						Title = "SpecIF:Stereotype",
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

			operationResource.Properties.Add(GetVisibilityProperty(operation.Visibility, operation.MethodGUID, eaElement.Modified, eaElement.Author));

			return operationResource;
		}

		private Resource ConvertOperationParameter(EAAPI.Parameter parameter, EAAPI.Element eaElement)
		{
			Resource result = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(parameter.ParameterGUID),
				ResourceClass = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = 1,
				Title = parameter.Name
			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = "dcterms:title",
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
					Title = "dcterms:description",
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
					Title = "dcterms:type",
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

		public Resource GetUmlElementResource(Key resourceKey)
		{
			Resource result = null;

			string eaGUID = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(resourceKey.ID);

			EAAPI.Element element = _repository.GetElementByGuid(eaGUID);

			if(element != null)
			{
				result = ConvertElement(element);
			}

			return result;
		}

		public Resource GetDiagramResource(Key resourceKey)
		{
			Resource result = null;

			string diagramXHTML = "";

			string eaGUID = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(resourceKey.ID);

			EAAPI.Diagram diagram = null;

			try
			{
				 diagram = _repository.GetDiagramByGuid(eaGUID) as EAAPI.Diagram;
			}
			catch(Exception)
			{ }

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
						ResourceClass = new Key("RC-Diagram", 1),
						ChangedAt = diagram.ModifiedDate,
						ChangedBy = diagram.Author,
						Revision = 1,
						Title = diagram.Name
					};

					result.Properties = new List<Property>();

					result.Properties.Add(
						new Property()
						{
							Title = "dcterms:title",
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
							Title = "dcterms:description",
							PropertyClass = new Key("PC-Text", 1),
							Value = new Value
							{
								LanguageValues = new List<LanguageValue>
								{
							new LanguageValue
							{
								Text = diagramXHTML + "\r\n<p>" + diagram.Notes + "</p>"
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
							Title = "SpecIF:Notation",
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

		public List<Statement> GetAllStatementsForResource(Key resourceKey)
		{
			List<Statement> result = new List<Statement>();

			string eaGUID = EaSpecIfGuidConverter.ConvertSpecIfGuidToEaGuid(resourceKey.ID);

			EAAPI.Element element = _repository.GetElementByGuid(eaGUID);

			if (element != null)
			{
				string elementType = element.Type;

				// classifier
				Statement classifierStatement = null;

				if (elementType == "Port" || elementType == "Part" || elementType == "ActionPin" || elementType == "ActivityParameter")
				{
					classifierStatement = GetClassifierStatement(element.PropertyType, resourceKey);
				}
				else
				{
					classifierStatement = GetClassifierStatement(element.ClassifierID, resourceKey);
				}

				if(classifierStatement != null)
				{
					result.Add(classifierStatement);
				}

				// sub elements
				result.AddRange(GetSubelementStatements(element, resourceKey));

				// attributes
				result.AddRange(GetAttributeStatements(element, resourceKey));

				// operations
				result.AddRange(GetOperationStatements(element, resourceKey));

				// uml connectors
				result.AddRange(ConvertUmlConnectors(element));

				
			}

			return result;
		}

		private Resource ConvertElement(EAAPI.Element eaElement)
		{
			Resource result = null;

			string elementType = eaElement.Type;

			if (elementType != "Requirement")
			{
				string resourceClass = GetResourceClassForElementType(elementType);

				Resource elementResource = new Resource()
				{
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID),
					ResourceClass = new Key(resourceClass, 1),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = 1,
					Title = eaElement.Name
				};

				elementResource.Properties = new List<Property>();

				elementResource.Properties.Add(
					new Property()
					{
						Title = "dcterms:title",
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
						Title = "dcterms:description",
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
						Title = "SpecIF:Stereotype",
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
						Title = "dcterms:type",
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
				}

				elementResource.Properties.Add(GetVisibilityProperty(eaElement.Visibility, eaElement.ElementGUID, eaElement.Modified, eaElement.Author));
				
				result = elementResource;
			}

			return result;
		}

		private Resource ConvertAttribute(EAAPI.Attribute attribute, EAAPI.Element eaElement)
		{

			Resource attributeResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID),
				ResourceClass = new Key("RC-UML_ActiveElement", 1),
				ChangedAt = eaElement.Modified,
				ChangedBy = eaElement.Author,
				Revision = 1,
				Title = attribute.Name
			};

			attributeResource.Properties = new List<Property>();

			attributeResource.Properties.Add(
				new Property()
				{
					Title = "dcterms:title",
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
					Title = "dcterms:description",
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
					Title = "dcterms:type",
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
						Title = "rdf:value",
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

			attributeResource.Properties.Add(GetVisibilityProperty(attribute.Visibility, attribute.AttributeGUID, eaElement.Modified, eaElement.Author));

			return attributeResource;
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
					ResourceClass = new Key("RC-UML_ActiveElement", 1),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author,
					Revision = 1,
					Title = tagName
				};

				tagResource.Properties = new List<Property>();

				tagResource.Properties.Add(
					new Property()
					{
						Title = "dcterms:title",
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
						Title = "dcterms:description",
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
						Title = "dcterms:type",
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
						Title = "rdf:value",
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

				if(tagStereotype != "")
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
						Title = "SpecIF:Stereotype",
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
					Title = "SpecIF:contains",
					StatementClass = new Key("SC-contains"),
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
					Title = "SpecIF:contains",
					StatementClass = new Key("SC-contains"),
					StatementSubject = new Key(elementResource.ID, elementResource.Revision),
					StatementObject = new Key(subElementSpecIfID, 1)
				};

				result.Add(attributeReferenceStatement);

			} // for

			return result;
		}

		private List<Statement> GetAttributeStatements(EAAPI.Element eaElement, Key elementResource)
		{
			List<Statement> result = new List<Statement>();

			string type = eaElement.Type;

			if(type == "Class" || type == "Component" || type == "State" || type == "Activity")
			{
				
				for(short counter = 0; counter < eaElement.Attributes.Count; counter++)
				{
					EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

					string attributeSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(attribute.AttributeGUID);

					// add UML:AtributeReference statement
					Statement attributeReferenceStatement = new Statement()
					{
						Title = "SpecIF:contains",
						StatementClass = new Key("SC-contains"),
						StatementSubject = new Key(elementResource.ID, elementResource.Revision),
						StatementObject = new Key(attributeSpecIfID, 1)
					};

					result.Add(attributeReferenceStatement);

				} // for
			} // if(class or component ...

			return result;
		}

		private List<Statement> GetOperationStatements(EAAPI.Element eaElement, Key elementResource)
		{
			List<Statement> result = new List<Statement>();

			string type = eaElement.Type;

			if (type == "Class" || type == "Component" || type == "State" || type == "Activity")
			{

				for (short counter = 0; counter < eaElement.Methods.Count; counter++)
				{
					EAAPI.Method method = eaElement.Methods.GetAt(counter) as EAAPI.Method;

					string operationSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(method.MethodGUID);

					// add statement
					Statement operationReferenceStatement = new Statement()
					{
						Title = "SpecIF:contains",
						StatementClass = new Key("SC-contains"),
						StatementSubject = new Key(elementResource.ID, elementResource.Revision),
						StatementObject = new Key(operationSpecIfID, 1)
					};

					result.Add(operationReferenceStatement);

				} // for
			} // if(class or component ...

			return result;
		}

		private List<Statement> GetElementTaggedValueStatements(EAAPI.Element eaElement, Key elementResource)
		{
			List<Statement> result = new List<Statement>();

			string type = eaElement.Type;

			for (short counter = 0; counter < eaElement.TaggedValues.Count; counter++)
			{
				EAAPI.TaggedValue tag = eaElement.TaggedValues.GetAt(counter) as EAAPI.TaggedValue;

				string tagSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(tag.PropertyGUID);

				// add statement
				Statement operationReferenceStatement = new Statement()
				{
					Title = "SpecIF:contains",
					StatementClass = new Key("SC-contains"),
					StatementSubject = new Key(elementResource.ID, elementResource.Revision),
					StatementObject = new Key(tagSpecIfID, 1)
				};

				result.Add(operationReferenceStatement);

			} // for
			

			return result;
		}

		private List<Statement> ConvertUmlConnectors(EAAPI.Element eaElement)
		{
			List<Statement> result = new List<Statement>();

			for (short counter = 0; counter < eaElement.Connectors.Count; counter++)
			{
				EAAPI.Connector connectorEA = eaElement.Connectors.GetAt(counter) as EAAPI.Connector;

				EAAPI.Element sourceElement = _repository.GetElementByID(connectorEA.ClientID);
				EAAPI.Element targetElement = _repository.GetElementByID(connectorEA.SupplierID);

				string sourceID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(sourceElement.ElementGUID);
				string targetID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(targetElement.ElementGUID);

				string connectorID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorEA.ConnectorGUID);

				Statement umlRelationship = new Statement()
				{
					ID = connectorID,
					Revision = 1,
					Title = "UML:Relationship",
					StatementClass = new Key("SC-UML_Relationship"),
					StatementSubject = new Key(sourceID, 1),
					StatementObject = new Key(targetID, 1)
				};

				umlRelationship.Properties = new List<Property>();

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = "dcterms:title",
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
						Title = "dcterms:description",
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
						Title = "dcterms:type",
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
						Title = "UML:ConnectorDirection",
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

				umlRelationship.Properties.Add(
					new Property()
					{
						Title = "UML:ConnectorSourceRole",
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
						Title = "UML:ConnectorTargetRole",
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
						Title = "UML:ConnectorSourceMultiplicity",
						PropertyClass = new Key("PC-UML_ConnectorSourceMultipilcity", 1),
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
						Title = "UML:ConnectorTargetMultiplicity",
						PropertyClass = new Key("PC-UML_ConnectorTargetMultipilcity", 1),
						Value = new Value
						{
							LanguageValues = new List<LanguageValue>
							{
									new LanguageValue
									{
										Text = connectorEA.ClientEnd.Cardinality                                    }
							}
						},
						ID = connectorID + "_TARGET_MULTIPLICITY"
					}
					);

				

				result.Add(umlRelationship);
				
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
						Title = "rdf:type",
						StatementClass = new Key("SC-Classifier", 1),
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
			result = new Statement()
			{
				Title = "SpecIF:contains",
				StatementClass = new Key("SC-contains", 1),
				StatementSubject = new Key(containerSpecIfID, 1),
				StatementObject = new Key(subElementSpecIfID, 1)
			};

			return result;
		}

		private Property GetVisibilityProperty(string visibility, string ID, DateTime changedAt, string changedBy)
		{
			Property result = new Property()
			{
				ID = ID,
				Revision = 1,
				PropertyClass = new Key("PC-UML_VisibilityKind", 1),
				Title = "UML:VisibilityKind",
				Value = new Value(),
				ChangedAt = changedAt,
				ChangedBy = changedBy
			};

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

		private string GetResourceClassForElementType(string type)
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
				case "Object":
				case "Actor":
				case "Event":
				case "TaggedValue":
				case "RunState":
				case "UseCase":

					result = "RC-UML_ActiveElement";
				break;

				case "Package":

					result = "RC-UML_Package";
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

		private void InitializePrimitiveTypes()
		{
			Resource integerResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{239A1D1D-046D-4902-87B1-4D86FB6B7D76}"),
				"Integer",
				"An instance of Integer is a value in the (infinite) set of integers (…-2, -1, 0, 1, 2…).");

			_primitiveTypes.Add(integerResource.Title, integerResource);

			Resource booleanResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{26844644-6F52-40A8-BDA8-43A61276B197}"),
				"Boolean",
				"An instance of Boolean is one of the predefined values true and false.");

			_primitiveTypes.Add(booleanResource.Title, booleanResource);

			Resource stringResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{4CEEB645-62E8-44B0-A97E-694FF01C25E4}"),
				"String",
				@"An instance of String defines a sequence of characters. Character sets may include non-Roman
alphabets.The semantics of the string itself depends on its purpose; it can be a comment,
computational language expression, OCL expression, etc.");

			_primitiveTypes.Add(stringResource.Title, stringResource);

			Resource unlimitedNaturalResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{C35A5389-2CBF-47FB-B987-A540E71B0BFC}"),
				"UnlimitedNatural",
				@"An instance of UnlimitedNatural is a value in the (infinite) set of natural numbers (0, 1, 2…) plus
unlimited. The value of unlimited is shown using an asterisk (‘*’). UnlimitedNatural values are
typically used to denote the upper bound of a range, such as a multiplicity; unlimited is used
whenever the range is specified to have no upper bound.");

			_primitiveTypes.Add(unlimitedNaturalResource.Title, unlimitedNaturalResource);

			Resource realResource = CreatePrimitiveTypeResource(
				EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid("{ABA6441D-268A-457D-A775-484C7301AF21}"),
				"Real",
				@"An instance of Real is a value in the (infinite) set of real numbers. Typically an implementation
will internally represent Real numbers using a floating point standard such as ISO/IEC/IEEE
60559:2011 (whose content is identical to the predecessor IEEE 754 standard).");

			_primitiveTypes.Add(realResource.Title, realResource);
		}

		private Resource CreatePrimitiveTypeResource(string id, 
													 string title,
													 string description)
		{
			Resource result = new Resource()
			{
				ID = id,
				Revision = 1,
				Title = title,
				ResourceClass = new Key(GetResourceClassForElementType("PrimitiveType"), 1),
				ChangedAt = new DateTime(2019, 4, 7),
				ChangedBy = "oalt"

			};

			result.Properties = new List<Property>();

			result.Properties.Add(
				new Property()
				{
					Title = "dcterms:title",
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
					Title = "dcterms:description",
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
					Title = "dcterms:type",
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
