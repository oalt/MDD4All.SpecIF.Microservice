using MDD4All.EnterpriseArchitect.DataModels;
using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EAAPI = EA;
using EADM = MDD4All.EnterpriseArchitect.DataModels.Contracts;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    public class SpecificationToHierarchyConverter
    {

        private static bool _searchesAdded = false;

        private EAAPI.Repository _repository;

        public SpecificationToHierarchyConverter(EAAPI. Repository repository)
        {
            _repository = repository;

            if(!_searchesAdded)
            {
                _repository.AddDefinedSearches(Searches.SPECIFICATION_PACKAGES);
                _repository.AddDefinedSearches(Searches.PACKAGE_ELEMENTS_SEARCH);
            }
        }

        public List<Node> GetAllHierarchies()
        {
            List<Node> result = new List<Node>();

            EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecificationPackages", "");

            if(searchResult.Count > 0)
            {
                for(short counter = 0; counter < searchResult.Count; counter++)
                {
                    EAAPI.Element packageElement = searchResult.GetAt(counter) as EAAPI.Element;

                    EAAPI.Package package = _repository.GetPackageByGuid(packageElement.ElementGUID);

                    string xml = _repository.SQLQuery("select * from t_object where t_object.Package_ID = " + package.PackageID);

                    XElement rootElement = XElement.Parse(xml);

                    XElement datasetElement = rootElement.Element("Dataset_0");

                    XElement dataElement = datasetElement.Element("Data");

                    IEnumerable<XElement> rows = dataElement.Elements("Row");

                    List<EADM.Element> specificationElements = new List<EADM.Element>();

                    foreach(XElement row in rows)
                    {
                        EADM.Element element = new ElementDataModel(row);

                        // tagged values
                        string taggedValueXml = _repository.SQLQuery("select * from t_objectproperties where Object_ID = " + element.ElementID);

                        XElement taggedValueRootElement = XElement.Parse(taggedValueXml);

                        XElement taggedValueDatasetElement = taggedValueRootElement.Element("Dataset_0");

                        if (taggedValueDatasetElement != null)
                        {
                            XElement taggedValueDataElement = taggedValueDatasetElement.Element("Data");

                            IEnumerable<XElement> taggedValueRows = taggedValueDataElement.Elements("Row");

                            foreach (XElement taggedValueRow in taggedValueRows)
                            {
                                EADM.TaggedValue taggedValue = new TaggedValueDataModel(taggedValueRow);

                                element.TaggedValues.Add(taggedValue);
                            }
                        }

                        specificationElements.Add(element);
                    }

                    EADM.Element specificationPackageElement = new ElementDataModel(packageElement);

                    CreateEaHierarchy(specificationElements, specificationPackageElement);

                    Node rootNode = new Node();

                    ConvertEaDataToSpecifNodeRecursively(specificationPackageElement, rootNode);

                    result = rootNode.Nodes;

                }
            }

            return result;
        }

        private void ConvertEaDataToSpecifNodeRecursively(EADM.Element currentElement, Node parentNode)
        {

            string eaSpecIfID = EaSpecIfGuidConverter.ConvertEaGuidToSpecIfGuid(currentElement.ElementGUID);

            string specIfResourceID = eaSpecIfID;
            string specIfResourceRevision = "1";


            if(currentElement.TaggedValues.Exists(tv => tv.Name == "specifId"))
            {
                specIfResourceID = currentElement.TaggedValues.First(tv => tv.Name == "specifId").Value;
                specIfResourceRevision = currentElement.TaggedValues.First(tv => tv.Name == "specifRevision").Value;
            }

            Node node = new Node()
            {
                ResourceReference = new Key(specIfResourceID, specIfResourceRevision),
                Description = currentElement.ToString(),
                ID = eaSpecIfID + "_Node"
            };

            parentNode.Nodes.Add(node);

            foreach(EADM.Element childElement in currentElement.Elements)
            {
                ConvertEaDataToSpecifNodeRecursively(childElement, node);
            }
        }

        private EADM.Element CreateEaHierarchy(List<EADM.Element> elements, EADM.Element rootElement)
        {
            EADM.Element result = rootElement;

            CreateEaHierarchyRecusrsively(elements, rootElement, 0);

            return result;
                
        }

        private void CreateEaHierarchyRecusrsively(List<EADM.Element> elements, EADM.Element parent, int parentID)
        {
            List<EADM.Element> childElements = elements.FindAll(el => el.ParentID == parentID).OrderBy(x => x.TreePos).ToList();

            parent.Elements.AddRange(childElements);

            foreach(EADM.Element childElement in childElements)
            {
                CreateEaHierarchyRecusrsively(elements, childElement, childElement.ElementID);
            }
        }
    }
}
