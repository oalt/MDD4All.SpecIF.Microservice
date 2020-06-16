using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using EAAPI = EA;
using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataIntegrator.EA.Extensions;
using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.DataIntegrator.EA
{
    public class ResourceToElementConverter
    {
        public EAAPI.Element AddResource(EAAPI.Element parentEaElement, Resource resourceToAdd, 
                                         EAAPI.Repository repository,
                                         ISpecIfMetadataReader metadataReader)
        {
            EAAPI.Element result = parentEaElement;

            EAAPI.Element newEaElement = null;

            if(resourceToAdd.Class.ID == "RC-Requirement")
            {
                newEaElement = parentEaElement.AddEmbeddedElement(repository, "Unsynced Requirement", "Requirement");

                newEaElement.SetRequirementDataFromSpecIF(resourceToAdd, metadataReader);
            }

            if (newEaElement != null)
            {
                result = newEaElement;
            }

            return result;
        }
    }
}
