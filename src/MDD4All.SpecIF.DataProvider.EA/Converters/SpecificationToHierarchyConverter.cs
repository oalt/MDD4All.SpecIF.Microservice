using MDD4All.EAFacade.DataAccess.Cached;
using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;
using System.Linq;
using EAAPI = EA;
using EADM = MDD4All.EAFacade.DataModels.Contracts;

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

                    CachedDataProvider eaCacheDataProvider = new CachedDataProvider(_repository);

                    EADM.Element specificationPackageElement = eaCacheDataProvider.GetCachedSpecification(package);

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
                Description = new List<MultilanguageText> {
                    new MultilanguageText(currentElement.ToString())
                },
                ID = eaSpecIfID + "_Node"
            };

            parentNode.Nodes.Add(node);

            foreach(EADM.Element childElement in currentElement.Elements)
            {
                ConvertEaDataToSpecifNodeRecursively(childElement, node);
            }
        }

       
    }
}
