using MDD4All.EnterpriseArchitect.Manipulations;
using MDD4All.SpecIF.DataIntegrator.Contracts;
using MDD4All.SpecIF.DataIntegrator.EA.Extensions;
using MDD4All.SpecIF.DataIntegrator.KafkaListener;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.Contracts;
using MDD4All.SpecIF.DataProvider.WebAPI;
using System;
using EAAPI = EA;

namespace MDD4All.SpecIF.DataIntegrator.EA
{
    public class EaDataIntegrator
    {
        
        private EAAPI.Repository _repository;
        private ISpecIfMetadataReader _metadataReader;

        private AbstractSpecIfEventListener _specIfEventListener;

        public EaDataIntegrator(EAAPI.Repository repository, ISpecIfMetadataReader metadataReader)
        {
            _repository = repository;
            _metadataReader = metadataReader;

            _specIfEventListener = new KafkaSpecIfEventListener();

            _specIfEventListener.SpecIfEventReceived += SpecIfEventReceived;

            _repository.CreateOutputTab("Kafka");

            _repository.EnsureOutputVisible("Kafka");

            AddSearches();

            _specIfEventListener.StartListening();
        }

        private void SpecIfEventReceived(object sender, SpecIfEventArgs args)
        {
            Resource receivedEvent = args.specIfEvent;

            if (receivedEvent.Class != null && receivedEvent.Class.ID == "RC-SpecIfEvent" && receivedEvent.Class.Revision == "1")
            {
                string apiURL = receivedEvent.GetPropertyValue("SpecIF:apiURL", _metadataReader);

                string resourceID = receivedEvent.GetPropertyValue("SpecIF:id", _metadataReader);

                string resourceRevision = receivedEvent.GetPropertyValue("SpecIF:revision", _metadataReader);

                string classID = receivedEvent.GetPropertyValue("SpecIF:classId", _metadataReader);

                string eventType = receivedEvent.GetPropertyValue("SpecIF:specifEventType", _metadataReader);

                string projectID = receivedEvent.GetPropertyValue("SpecIF:project", _metadataReader);

                if (classID == "RC-Requirement")
                {
                    SpecIfWebApiDataReader specIfWebApiDataReader = new SpecIfWebApiDataReader(apiURL);

                    Resource changedResource = specIfWebApiDataReader.GetResourceByKey(new Key
                    {
                        ID = resourceID,
                        Revision = resourceRevision
                    });

                    if (changedResource != null)
                    {
                        string identifier = changedResource.GetPropertyValue("dcterms:identifier", _metadataReader);

                        if (eventType == "V-SET-ResourceCreated")
                        {
                            WriteLog("SpecIF-Requirement create event [" + identifier + "].");
                            CreateEaRequirement(changedResource, projectID);
                        }
                        else if (eventType == "V-SET-ResourceUpdated")
                        {
                            WriteLog("SpecIF-Requirement changed event [" + identifier + "].");
                            UpdateData(changedResource, projectID);
                        }
                    }
                }
            }
        }

        private void WriteLog(string logMessage)
        {
            logMessage = "[" + DateTime.Now.ToString() + "] " + logMessage;
            _repository.WriteOutput("Kafka", logMessage, 0);
        }

        private void UpdateData(Resource changedResource, string projectID)
        {
            EAAPI.Collection searchResult = _repository.GetElementsByQuery("SpecIfElement", changedResource.ID);

            if (searchResult.Count > 0)
            {
                EAAPI.Element eaElement = searchResult.GetAt(0) as EAAPI.Element;

                eaElement.SetRequirementDataFromSpecIF(changedResource, _metadataReader);

                _repository.AdviseElementChange(eaElement.ElementID);
            }
            else
            {
                // create new element
                CreateEaRequirement(changedResource, projectID);
            }
        }

        private void CreateEaRequirement(Resource newResource, string projectID)
        {
            EAAPI.Collection searchResult = _repository.GetElementsByQuery("Jira Requirements Folder", projectID);

            if (searchResult.Count > 0)
            {
                EAAPI.Element packageElement = searchResult.GetAt(0) as EAAPI.Element;

                EAAPI.Package package = _repository.GetPackageByGuid(packageElement.ElementGUID);

                EAAPI.Element eaRequirement = package.AddElement("Unsynchronized requirement", "Requirement");

                eaRequirement.SetRequirementDataFromSpecIF(newResource, _metadataReader);

                _repository.AdviseElementChange(eaRequirement.ElementID);
            }
            else
            {
                // TODO Create packages
            }
        }


        private void AddSearches()
        {
            _repository.AddDefinedSearches(Searches.SPECIF_ELEMENT_SEARCH);
            _repository.AddDefinedSearches(Searches.JIRA_REQUIREMENTS_FOLDER_SEARCH);
        }
    }
}
