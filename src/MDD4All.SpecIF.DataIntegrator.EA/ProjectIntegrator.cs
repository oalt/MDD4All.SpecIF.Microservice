using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;
using EAAPI = EA;
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataIntegrator.EA.Extensions;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.EA.Converters;
using MDD4All.SpecIF.DataModels.Manipulation;
using NLog;

namespace MDD4All.SpecIF.DataIntegrator.EA
{
    public class ProjectIntegrator
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static bool _searchesDefined = false;

        private EAAPI.Repository _repository;
        private ISpecIfMetadataReader _metadataReader;
        private ISpecIfDataReader _specIfDataReader;

        public ProjectIntegrator(EAAPI.Repository repository, 
                                 ISpecIfMetadataReader metadataReader,
                                 ISpecIfDataReader specIfDataReader)
        {
            _repository = repository;
            _metadataReader = metadataReader;
            _specIfDataReader = specIfDataReader;

            AddSearches();
        }

        public void SynchronizeProjectRoots()
        {
            EAAPI.Package integrationRootPackage = _repository.GetTreeSelectedPackage();

            if(integrationRootPackage != null)
            {
                string apiURL = integrationRootPackage.Element.GetTaggedValueString("specifApiUrl");

                if(!string.IsNullOrEmpty(apiURL))
                {
                    List<ProjectDescriptor> projectDescriptors = _specIfDataReader.GetProjectDescriptions();

                    foreach (ProjectDescriptor projectDescriptor in projectDescriptors)
                    {
                        EAAPI.Collection projectSearchResult = _repository.GetElementsByQuery("SpecIfProjectByID", projectDescriptor.ID);

                        EAAPI.Package projectPackage;

                        if(projectSearchResult.Count > 0)
                        {
                            EAAPI.Element packageElement = projectSearchResult.GetAt(0) as EAAPI.Element;

                            projectPackage = _repository.GetPackageByGuid(packageElement.ElementGUID);
                        }
                        else // create new package
                        {
                            projectPackage = integrationRootPackage.AddChildPackage(projectDescriptor.Title.ToString());
                        }

                        projectPackage.SetProjectDataFromSpecIF(projectDescriptor);
                    }
                }
            }

        }

        public void SynchronizeProjectHierarchyRoots()
        {
            EAAPI.Package projectPackage = _repository.GetTreeSelectedPackage();

            if(projectPackage != null && projectPackage.Element.Stereotype == "specif project" && projectPackage.PackageID != 0)
            {
                EAAPI.Package parentPackage = _repository.GetPackageByID(projectPackage.ParentID);

                if(parentPackage != null && parentPackage.Element.Stereotype == "specif integration")
                {
                    string apiURL = parentPackage.Element.GetTaggedValueString("specifApiUrl");

                    string projectID = projectPackage.Element.GetTaggedValueString("specifProjectID");

                    if(!string.IsNullOrEmpty(apiURL) && !string.IsNullOrEmpty(projectID))
                    {
                        

                        List<Node> hierarchyRoots = _specIfDataReader.GetAllHierarchyRootNodes(projectID);

                        foreach (Node rootNode in hierarchyRoots)
                        {

                            EAAPI.Package hierarchyRootPackage;

                            EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecIfHierarchyPackageByID", rootNode.ID);
                            
                            if(searchResult.Count > 0)
                            {
                                EAAPI.Element packageElement = searchResult.GetAt(0) as EAAPI.Element;

                                hierarchyRootPackage = _repository.GetPackageByGuid(packageElement.ElementGUID);
                                
                            }
                            else
                            {
                                hierarchyRootPackage = projectPackage.AddChildPackage("New Hierarchy");
                            }

                            Resource hierarchyRootResource = _specIfDataReader.GetResourceByKey(rootNode.ResourceReference);

                            if(hierarchyRootResource != null)
                            {
                                hierarchyRootPackage.SetHierarchyRootPackegeFromSpecIF(hierarchyRootResource, rootNode, _metadataReader);
                            }

                        }
                    }
                }
            }
        }

        public void SynchronizeHierarchyResources()
        {
            EAAPI.Package hierarchyPackage = _repository.GetTreeSelectedPackage();

            if (hierarchyPackage != null && hierarchyPackage.Element.Stereotype == "specif hierarchy" && hierarchyPackage.ParentID != 0)
            {
                EAAPI.Package projectPackage = _repository.GetPackageByID(hierarchyPackage.ParentID);

                if (projectPackage.ParentID != 0)
                {
                    EAAPI.Package integrationPackage = _repository.GetPackageByID(projectPackage.ParentID);

                    if(integrationPackage.Element.Stereotype == "specif integration")
                    {
                        string apiURL = integrationPackage.Element.GetTaggedValueString("specifApiUrl");

                        string hierarchyID = hierarchyPackage.Element.GetTaggedValueString("rootNodeID");

                        if(!string.IsNullOrEmpty(apiURL) && !string.IsNullOrEmpty(hierarchyID))
                        {
                            Node hierarchy = _specIfDataReader.GetHierarchyByKey(new Key(hierarchyID));

                            if(hierarchy != null)
                            {
                                SynchronizeHierarchyResourcesRecursively(hierarchy, hierarchyPackage.Element);
                            }

                        }
                    }
                }
            }
        }

        private void SynchronizeHierarchyResourcesRecursively(Node currentNode, EAAPI.Element parentElement)
        {
            Resource resource = _specIfDataReader.GetResourceByKey(currentNode.ResourceReference);

            if (resource != null)
            {
                EAAPI.Element eaSpecIfElement = null;

                EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecIfElement", currentNode.ResourceReference.ID);

                if (searchResult.Count > 0)
                {
                    eaSpecIfElement = searchResult.GetAt(0) as EAAPI.Element;

                    string eaRevision = eaSpecIfElement.GetTaggedValueString("specifRevision");

                    if (eaRevision != resource.Revision)
                    {
                        logger.Info("Synchronizing existing resource " + resource.ID);
                        eaSpecIfElement.SetRequirementDataFromSpecIF(resource, _metadataReader);
                    }
                }
                // if an EA GUID is available in the alternative IDs, try to synchronize existing element by using the EA GUID.
                else if (resource.AlternativeIDs != null && resource.AlternativeIDs.Count > 0)
                {
                    foreach (AlternativeId alternativeId in resource.AlternativeIDs)
                    {
                        if (alternativeId.Project != null && alternativeId.Project.StartsWith("Enterprise Architect"))
                        {
                            if (!string.IsNullOrEmpty(alternativeId.ID) && alternativeId.ID.StartsWith("{"))
                            {
                                eaSpecIfElement = _repository.GetElementByGuid(alternativeId.ID);
                                break;
                            }
                        }
                    }

                    if (eaSpecIfElement != null)
                    {
                        logger.Info("Synchronizing existing resource " + resource.ID);
                        eaSpecIfElement.SetRequirementDataFromSpecIF(resource, _metadataReader);
                    }
                    
                }

                // if no element was found by SpecIF ID and by alternative ID, add a new element
                if (eaSpecIfElement == null)
                {
                    ResourceToElementConverter resourceToElementConverter = new ResourceToElementConverter();
                    logger.Info("Adding new resource " + resource.ID);
                    resourceToElementConverter.AddResource(parentElement, resource, _repository, _metadataReader);
                }

                foreach (Node childNode in currentNode.Nodes)
                {
                    SynchronizeHierarchyResourcesRecursively(childNode, parentElement);
                }
            }
        }

        public void SynchronizeSingleElement(EAAPI.Element element)
        {
            string specifID = element.GetTaggedValueString("specifId");
            string specifRevision = element.GetTaggedValueString("specifRevision");

            if (!string.IsNullOrEmpty(specifID) && !string.IsNullOrEmpty(specifRevision))
            {
                Key key = new Key(specifID, specifRevision);

                logger.Info("Synchronizing element: " + key.ToString());

                Resource resource = _specIfDataReader.GetResourceByKey(key);

                if (resource != null)
                {
                    
                    element.SetRequirementDataFromSpecIF(resource, _metadataReader);
                    logger.Info("Synchronization finished.");
                }
                else
                {
                    logger.Error("Error getting SpecIF data.");
                }
            }
        }

        public Resource AddRequirementToSpecIF(ISpecIfDataWriter requirementWriter, EAAPI.Element reqiuirement, string projectID)
        {
            Resource result = null;

            EaUmlToSpecIfConverter eaToSpecIfConverter = new EaUmlToSpecIfConverter(_repository, _metadataReader);

            Resource resource = eaToSpecIfConverter.ConvertElement(reqiuirement);

            result = requirementWriter.SaveResource(resource, projectID);

            if (result != null)
            {
                reqiuirement.SetTaggedValueString("specifId", result.ID, false);
                reqiuirement.SetTaggedValueString("specifRevision", result.Revision, false);
                string key = result.GetPropertyValue("dcterms:identifier", _metadataReader);

                reqiuirement.SetTaggedValueString("Identifier", key, false);

                

            }

            return result;
        }

        private void AddSearches()
        {
            if(_searchesDefined == false)
            {
                _repository.AddDefinedSearches(Searches.SEARCH_SPECIF_PROJECT);
                _repository.AddDefinedSearches(Searches.SEARCH_HIERARCHY_ROOT_PACKAGE);
                _repository.AddDefinedSearches(Searches.SPECIF_ELEMENT_SEARCH);
                _repository.AddDefinedSearches(Searches.SPECIF_INTEGRATION_PACKAGE_SEARCH);

                _searchesDefined = true;
            }
        }
    }
}
