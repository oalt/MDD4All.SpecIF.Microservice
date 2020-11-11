using MDD4All.EnterpriseArchitect.DataModels;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using EAAPI = EA;
using EADM = MDD4All.EnterpriseArchitect.DataModels.Contracts;

namespace MDD4All.SpecIF.DataProvider.EA.Cache
{
    public class EaCacheDataProvider
    {
        private EAAPI.Repository _repository;

        public EaCacheDataProvider(EAAPI.Repository repository)
        {
            _repository = repository;
        }

        public EADM.Element GetCachedSpecification(EAAPI.Package specificationPackage)
        {
            string xml = _repository.SQLQuery("select * from t_object where t_object.Package_ID = " + specificationPackage.PackageID);

            XElement rootElement = XElement.Parse(xml);

            XElement datasetElement = rootElement.Element("Dataset_0");

            XElement dataElement = datasetElement.Element("Data");

            IEnumerable<XElement> rows = dataElement.Elements("Row");

            List<EADM.Element> specificationElements = new List<EADM.Element>();

            foreach (XElement row in rows)
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

            EADM.Element specificationPackageElement = new ElementDataModel(specificationPackage.Element);

            CreateEaHierarchy(specificationElements, specificationPackageElement);

            return specificationPackageElement;
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

            foreach (EADM.Element childElement in childElements)
            {
                CreateEaHierarchyRecusrsively(elements, childElement, childElement.ElementID);
            }
        }

        public List<EADM.Element> GetSpecificationElementList(EADM.Element rootElement)
        {
            List<EADM.Element> result = new List<EADM.Element>();
            
            GetSpecificationElementListRecursively(rootElement, result);

            return result;
        }

        private void GetSpecificationElementListRecursively(EADM.Element currentElement, List<EADM.Element> result)
        {
            result.Add(currentElement);

            foreach (EADM.Element childElement in currentElement.Elements)
            {
                GetSpecificationElementListRecursively(childElement, result);
            }
        }
    }


}
