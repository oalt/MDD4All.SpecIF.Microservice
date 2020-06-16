using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataIntegrator.EA.Extensions
{
    public static class EaDataIntegrationExtensions
    {
        public static void SetRequirementDataFromSpecIF(this EAAPI.Element eaRequirement, 
                                                        Resource resource, 
                                                        ISpecIfMetadataReader metadataReader)
        {
            string title = resource.GetPropertyValue("dcterms:title", metadataReader);

            string description = resource.GetPropertyValue("dcterms:description", metadataReader);

            string identifier = resource.GetPropertyValue("dcterms:identifier", metadataReader);

            eaRequirement.Name = title;

            eaRequirement.Notes = description;

            eaRequirement.Stereotype = "fmcreq";

            eaRequirement.Update();

            eaRequirement.SetTaggedValueString("specifId", resource.ID, false);

            eaRequirement.SetTaggedValueString("specifRevision", resource.Revision, false);

            eaRequirement.SetTaggedValueString("Identifier", identifier, false);
        }

        public static void SetProjectDataFromSpecIF(this EAAPI.Package projectPackage,
                                                    ProjectDescriptor project)
        {
            EAAPI.Element packageElement = projectPackage.Element;

            packageElement.Name = project.Title.ToString();

            packageElement.Stereotype = "specif project";

            packageElement.Update();
            projectPackage.Update();

            packageElement.SetTaggedValueString("specifProjectID", project.ID, false);

            
        }

        public static void SetHierarchyRootPackegeFromSpecIF(this EAAPI.Package hierarchyRootPackage,
                                                             Resource resource,
                                                             Node node,
                                                             ISpecIfMetadataReader metadataReader)
        {
            EAAPI.Element packageElement = hierarchyRootPackage.Element;

            string title = resource.GetPropertyValue("dcterms:title", metadataReader);

            hierarchyRootPackage.Name = title;
            hierarchyRootPackage.Update();

            packageElement.Stereotype = "specif hierarchy";

            packageElement.Update();
            

            packageElement.SetTaggedValueString("rootNodeID", node.ID, false);

            
        }
    }
}
