using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;
using MDD4All.SpecIF.DataModels;
using MDD4All.EnterpriseArchitect.Manipulations;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
	// TODO: stereotypes, tagged values, connector visibility, operations, run states

	public class EaUmlToSpecIfConverter
	{
		private EAAPI.Repository _repository;

		private DataModels.SpecIF _specIfResult;

		public EaUmlToSpecIfConverter(EAAPI.Repository repository)
		{
			_repository = repository;
		}

		public DataModels.SpecIF StartConversion(EAAPI.Package selectedPackage)
		{
			_specIfResult = new DataModels.SpecIF();

			_specIfResult.Generator = "SpecIFicator";
			_specIfResult.GeneratorVersion = "1.0";
			_specIfResult.Title = "UML data extracted from Sparx Enterprise Architect";

			Resource umlModelResource = new Resource()
			{
				Title = "UML:Model",
				ResourceClass = new Key("RC-UML_Model", 1)
			};

			umlModelResource.Properties = new List<Property>();

			umlModelResource.Properties.Add(
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
					ID = umlModelResource.ID + "_NAME"
				}
				);

			_specIfResult.Resources.Add(umlModelResource);

			ConvertModelRecursively(selectedPackage);


			return _specIfResult;
		}

		private void ConvertModelRecursively(EAAPI.Package currentPackage)
		{
			ConvertPackage(currentPackage);

			// TODO elements, embedded elements, create hierarchy

		}



		private Resource ConvertPackage(EAAPI.Package eaPackage)
		{
			Resource packageResource = new Resource()
			{
				ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaPackage.Element.ElementGUID),
				ResourceClass = new Key("RC-UML_PassiveElement", 1),
				ChangedAt = eaPackage.LastSaveDate,
				ChangedBy = eaPackage.Owner,
				Revision = 1,
				Title = eaPackage.Name
			};

			packageResource.Properties = new List<Property>();

			packageResource.Properties.Add(
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
								Text = eaPackage.Name
							}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaPackage.Element.ElementGUID + "_NAME"),
					ChangedAt = eaPackage.Element.Modified,
					ChangedBy = eaPackage.Element.Author
				}
				);

			packageResource.Properties.Add(
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
								Text = eaPackage.Notes
							}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaPackage.Element.ElementGUID + "_NOTES"),
					ChangedAt = eaPackage.Element.Modified,
					ChangedBy = eaPackage.Element.Author
				}
				);

			packageResource.Properties.Add(
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
								Text = "Package"
							}
						}
					},
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaPackage.Element.ElementGUID + "_TYPE"),
					ChangedAt = eaPackage.Element.Modified,
					ChangedBy = eaPackage.Element.Author
				}
				);

			_specIfResult.Resources.Add(packageResource);

			ConvertConnectors(eaPackage.Element);

			return packageResource;
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

				_specIfResult.Resources.Add(elementResource);

				ConvertAttributes(eaElement, elementResource);

				EAAPI.Element classifierEA = eaElement.GetClassifier(_repository);

				if (classifierEA != null)
				{
					ConvertClassifier(classifierEA, elementResource);
				}

				result = elementResource;
			}

			ConvertConnectors(eaElement);

			

			return result;
		}

		private void ConvertAttributes(EAAPI.Element eaElement, Resource elementResource)
		{
			string type = eaElement.Type;

			if(type == "Class" || type == "Component" || type == "State" || type == "Activity")
			{
				for(short counter = 0; counter < eaElement.Attributes.Count; counter++)
				{
					EAAPI.Attribute attribute = eaElement.Attributes.GetAt(counter) as EAAPI.Attribute;

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

					_specIfResult.Resources.Add(attributeResource);

					// add UML:AtributeReference statement
					Statement attributeReferenceStatement = new Statement()
					{
						Title = "UML:AtributeReference",
						StatementClass = new Key("SC-UML_AttributeReference"),
						StatementSubject = new Key(elementResource.ID, elementResource.Revision),
						StatementObject = new Key(attributeResource.ID, attributeResource.Revision)
					};

					_specIfResult.Statements.Add(attributeReferenceStatement);

					// classifier
					if(attribute.ClassifierID != 0)
					{
						EAAPI.Element attributeClassifier = _repository.GetElementByID(attribute.ClassifierID);

						if(attributeClassifier != null)
						{
							ConvertClassifier(attributeClassifier, attributeResource);
						}
					}

				} // for
			} // if(class or component ...


		}

		private void ConvertClassifier(EAAPI.Element classifierEA, Resource classifiedResource)
		{
			
			string classifierSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierEA.ElementGUID);

			Resource classifier = _specIfResult.Resources.Find(resource => resource.ID == classifierSpecIfID);
			
			if(classifier == null)
			{
				classifier = new Resource()
				{
					ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierEA.ElementGUID),
					ResourceClass = new Key(GetResourceClassForElementType(classifierEA.Type), 1),
					ChangedAt = classifierEA.Modified,
					ChangedBy = classifierEA.Author,
					Revision = 1,
					Title = classifierEA.Name
				};

				classifier.Properties = new List<Property>();

				classifier.Properties.Add(
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
										Text = classifierEA.Name
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierEA.ElementGUID + "_NAME"),
						ChangedAt = classifierEA.Modified,
						ChangedBy = classifierEA.Author
					}
					);

				classifier.Properties.Add(
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
										Text = classifierEA.Notes
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierEA.ElementGUID + "_NOTES"),
						ChangedAt = classifierEA.Modified,
						ChangedBy = classifierEA.Author
					}
					);

				classifier.Properties.Add(
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
										Text = classifierEA.Type
									}
							}
						},
						ID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(classifierEA.ElementGUID + "_TYPE"),
						ChangedAt = classifierEA.Modified,
						ChangedBy = classifierEA.Author
					}
					);

				_specIfResult.Resources.Add(classifier);

			}

			// add rdf:type statement
			Statement typeStatement = new Statement()
			{
				Title = "UML:AtributeReference",
				StatementClass = new Key("SC-UML_AttributeReference"),
				StatementSubject = new Key(classifiedResource.ID, classifiedResource.Revision),
				StatementObject = new Key(classifier.ID, classifier.Revision)
			};

			_specIfResult.Statements.Add(typeStatement);


		}

		private void ConvertConnectors(EAAPI.Element eaElement)
		{
			for(short counter = 0; counter < eaElement.Connectors.Count; counter++)
			{
				EAAPI.Connector connectorEA = eaElement.Connectors.GetAt(counter) as EAAPI.Connector;

				EAAPI.Element targetElement = _repository.GetElementByID(connectorEA.SupplierID);

				string sourceID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(eaElement.ElementGUID);
				string targetID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(targetElement.ElementGUID);

				string connectorID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(connectorEA.ConnectorGUID);

				Statement umlRelationship = _specIfResult.Statements.Find(sm => sm.ID == connectorID);

				if(umlRelationship == null)
				{
					umlRelationship = new Statement()
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
										Text = connectorEA.ClientEnd.Cardinality									}
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

					_specIfResult.Statements.Add(umlRelationship);
				}
			}
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
