using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataProvider.WebAPI;
using MDD4All.SpecIF.DataIntegrator.EA.Extensions;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.EA.Converters;
using MDD4All.SpecIF.DataProvider.Jira;
using MDD4All.SpecIF.DataModels.Manipulation;

namespace MDD4All.SpecIF.DataIntegrator.EA
{
    public class ProjectIntegrator
    {
        
        private static bool _searchesDefined = false;

        private EAAPI.Repository _repository;
        private ISpecIfMetadataReader _metadataReader;
        

        public ProjectIntegrator(EAAPI.Repository repository, ISpecIfMetadataReader metadataReader)
        {
            _repository = repository;
            _metadataReader = metadataReader;
        

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
                    SpecIfWebApiDataReader webApiDataReader = new SpecIfWebApiDataReader(apiURL);

                    List<ProjectDescriptor> projectDescriptors = webApiDataReader.GetProjectDescriptions();

                    

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
                        SpecIfWebApiDataReader webApiDataReader = new SpecIfWebApiDataReader(apiURL);

                        List<Node> hierarchyRoots = webApiDataReader.GetAllHierarchyRootNodes(projectID);

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

                            Resource hierarchyRootResource = webApiDataReader.GetResourceByKey(rootNode.ResourceReference);

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
                            SpecIfWebApiDataReader webApiDataReader = new SpecIfWebApiDataReader(apiURL);

                            Node hierarchy = webApiDataReader.GetHierarchyByKey(new Key(hierarchyID));

                            if(hierarchy != null)
                            {
                                SynchronizeHierarchyResourcesRecusrively(hierarchy, hierarchyPackage.Element, webApiDataReader);
                            }

                        }
                    }
                }
            }
        }

        private void SynchronizeHierarchyResourcesRecusrively(Node currentNode, EAAPI.Element parentElement, 
                                                              ISpecIfDataReader dataReader)
        {

            Resource resource = dataReader.GetResourceByKey(currentNode.ResourceReference);

            if (resource != null)
            {

                EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecIfElement", currentNode.ResourceReference.ID);

                EAAPI.Element eaSpecIfElement;

                if (searchResult.Count > 0)
                {
                    eaSpecIfElement = searchResult.GetAt(0) as EAAPI.Element;

                    string eaRevision = eaSpecIfElement.GetTaggedValueString("specifRevision");

                    if (eaRevision != resource.Revision)
                    {
                        eaSpecIfElement.SetRequirementDataFromSpecIF(resource, _metadataReader);
                    }
                }
                else
                {
                    ResourceToElementConverter resourceToElementConverter = new ResourceToElementConverter();

                    resourceToElementConverter.AddResource(parentElement, resource, _repository, _metadataReader);
                }

                foreach(Node childNode in currentNode.Nodes)
                {
                    SynchronizeHierarchyResourcesRecusrively(childNode, parentElement, dataReader);
                }
            }
        }

        public Resource AddRequirementToSpecIF(string url, LoginData loginData, EAAPI.Element reqiuirement, string projectID)
        {
            Resource result = null;

            SpecIfWebApiDataReader specIfWebApiDataReader = new SpecIfWebApiDataReader(url);

            
            SpecIfWebApiDataWriter webApiDataWriter = new SpecIfWebApiDataWriter(url, loginData, _metadataReader, specIfWebApiDataReader);



            EaToSpecIfConverter eaToSpecIfConverter = new EaToSpecIfConverter(_repository);

            Resource resource = eaToSpecIfConverter.ConvertElementToResource(reqiuirement);

            result = webApiDataWriter.SaveResource(resource, projectID);

            if (result != null)
            {
                reqiuirement.SetTaggedValueString("specifId", result.ID, false);
                reqiuirement.SetTaggedValueString("specifRevision", result.Revision, false);
                string key = result.GetPropertyValue("dcterms:identifier", _metadataReader);

                reqiuirement.SetTaggedValueString("identifier", key, false);
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
