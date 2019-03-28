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
	// TODO: stereotypes, tagged values, connector visibility, operations, run states

	public class EaUmlToSpecIfConverter
	{
		private EAAPI.Repository _repository;

		private Resource _modelResource;

		public EaUmlToSpecIfConverter(EAAPI.Repository repository)
		{
			_repository = repository;
		}

		public Node GetModelHierarchy(EAAPI.Package selectedPackage)
		{
			_modelResource = new Resource()
			{
				Title = "UML:Model",
				ResourceClass = new Key("RC-UML_Model", 1)
			};

			_modelResource.Properties = new List<Property>();

			_modelResource.Properties.Add(
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
								Text = "UML Model: " + selectedPackage.Name
							}
						}
					},
					ID = _modelResource.ID + "_NAME"
				}
				);

			

			Node result = new Node();

			result.ResourceReference = new Key(_modelResource.ID, _modelResource.Revision);

			Node childNode = new Node();

			result.Nodes.Add(childNode);

			GetModelHierarchyRecursively(selectedPackage, childNode);
			
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

				string specIfElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(element.ElementGUID);

				Node node = new Node()
				{
					ResourceReference = new Key(specIfElementID, 1)
				};

				currentNode.Nodes.Add(node);

				for (short embeddedElementCounter = 0; embeddedElementCounter < element.EmbeddedElements.Count; embeddedElementCounter++)
				{
					EAAPI.Element embeddedElement = element.EmbeddedElements.GetAt(embeddedElementCounter) as EAAPI.Element;

					string specIfEmbeddedElementID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(embeddedElement.ElementGUID);

					Node embeddedElementNode = new Node()
					{
						ResourceReference = new Key(specIfEmbeddedElementID, 1)
					};

					node.Nodes.Add(embeddedElementNode);
				}
			}


			//ConvertPackage(currentPackage);


			// TODO elements, embedded elements, create hierarchy

		}

		public void AddSpecIfDataBasedOnHierarchy(DataModels.SpecIF specIF)
		{
			specIF.Resources.Add(_modelResource);

			if (specIF.Hierarchies.Count > 0)
			{
				AddSpecIfDataBasedOnHierarchyRecursively(specIF.Hierarchies[0], specIF);
			}
		}

		private void AddSpecIfDataBasedOnHierarchyRecursively(Node currentNode, DataModels.SpecIF specIF)
		{
			Resource resource = GetUmlElementResource(currentNode.ResourceReference);

			if(resource == null) // is it a diagram GUID?
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
				AddSpecIfDataBasedOnHierarchyRecursively(childNode, specIF);
			}
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

				// attributes
				result.AddRange(GetAttributeStatements(element, resourceKey));

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
								Text = eaElement.Type
							}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_TYPE"),
						ChangedAt = eaElement.Modified,
						ChangedBy = eaElement.Author
					}
					);

				//_specIfResult.Resources.Add(elementResource);

				//ConvertAttributes(eaElement, elementResource);

				//EAAPI.Element classifierEA = eaElement.GetClassifier(_repository);

				//if (classifierEA != null)
				//{
				//	ConvertClassifier(classifierEA, elementResource);
				//}

				result = elementResource;
			}

			//ConvertConnectors(eaElement);

			

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
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NAME"),
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
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_NOTES"),
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
										Text = "Attribute"
									}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID + "_TYPE"),
					ChangedAt = eaElement.Modified,
					ChangedBy = eaElement.Author
				}
				);

			return attributeResource;
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
						Title = "UML:AtributeReference",
						StatementClass = new Key("SC-UML_AttributeReference"),
						StatementSubject = new Key(elementResource.ID, elementResource.Revision),
						StatementObject = new Key(attributeSpecIfID, 1)
					};

					result.Add(attributeReferenceStatement);

					// classifier
					//if(attribute.ClassifierID != 0)
					//{
					//	EAAPI.Element attributeClassifier = _repository.GetElementByID(attribute.ClassifierID);

					//	if(attributeClassifier != null)
					//	{
					//		ConvertClassifier(attributeClassifier, attributeResource);
					//	}
					//}

				} // for
			} // if(class or component ...

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
										Text = connectorEA.Type
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

				// TODO access type

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

					result = "RC-UML_PassiveElement";
				break;

				case "Activity":
				case "Action":
				case "Object":
				case "Actor":
				case "Event":
				case "Tag":
				case "RunState":
				case "UseCase":

					result = "RC-UML_ActiveElement";
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

	}
}
