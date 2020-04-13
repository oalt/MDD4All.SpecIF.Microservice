using Confluent.Kafka;
using MDD4All.Kafka.DataAccess;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.Contracts;
using System;
using System.Diagnostics;
using System.Threading;
using EAAPI = EA;
using MDD4All.SpecIF.DataModels.Manipulation;
using MDD4All.SpecIF.DataProvider.WebAPI;
using MDD4All.EnterpriseArchitect.Manipulations;

namespace MDD4All.SpecIF.Apps.EaIntegration
{
    public class EaKafkaIntegrator
    {
        private EAAPI.Repository _repository;

        private const string SPECIF_ELEMENT_SEARCH = "<?xml version=\"1.0\" encoding=\"windows-1252\"?><RootSearch><Search Name=\"SpecIfElement\" GUID=\"{79E272AF-088F-48f7-936A-21F74B83387A}\" PkgGUID=\"-1\" Type=\"1\" LnksToObj=\"0\" CustomSearch=\"0\" AddinAndMethodName=\"\"><SrchOn><RootTable Type=\"0\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"\" Hierarchy=\"t_object\"/><Table Join=\"t_object.Object_ID = t_objectproperties.Object_ID\" Type=\"0\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"\" Hierarchy=\"t_object.t_objectproperties\"/><Field Filter=\"t_objectproperties.[Value] = '&lt;Search Term&gt;'\" Text=\"&lt;Search Term&gt;\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"TagValue\" Hierarchy=\"t_objectproperties\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"Value\" Name=\"t_objectproperties.[Value]\"/></Field><Field Filter=\"t_objectproperties.Property = 'specifId'\" Text=\"specifId\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"TagValue\" Hierarchy=\"t_objectproperties\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"Property\" Name=\"t_objectproperties.Property\"/></Field></Table></RootTable></SrchOn><LnksTo/></Search></RootSearch>";
        private const string JIRA_REQUIREMENTS_FOLDER_SEARCH = "<?xml version=\"1.0\" encoding=\"windows-1252\"?><RootSearch><Search Name=\"Jira Requirements Folder\" GUID=\"{CD7B9A44-1132-4685-95B8-D9166C654703}\" PkgGUID=\"-1\" Type=\"0\" LnksToObj=\"0\" CustomSearch=\"0\" AddinAndMethodName=\"\"><SrchOn><RootTable Type=\"0\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"\" Hierarchy=\"t_object\"/><Field Filter=\"t_object.Stereotype Like '*jire requirements*'\" Text=\"jire requirements\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"Element\" Hierarchy=\"t_object\"/><Condition Display=\"Contains\" Type=\"Like\"/><FieldName Display=\"Stereotype\" Name=\"t_object.Stereotype\"/></Field><Table Join=\"t_object.Object_ID = t_objectproperties.Object_ID\" Type=\"0\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"\" Hierarchy=\"t_object.t_objectproperties\"/><Field Filter=\"t_objectproperties.[Value] = '&lt;Search Term&gt;'\" Text=\"&lt;Search Term&gt;\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"TagValue\" Hierarchy=\"t_objectproperties\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"Value\" Name=\"t_objectproperties.[Value]\"/></Field><Field Filter=\"t_objectproperties.Property = 'specifProjectID'\" Text=\"specifProjectID\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"TagValue\" Name=\"t_objectproperties\"/><TableHierarchy Display=\"TagValue\" Hierarchy=\"t_objectproperties\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"Property\" Name=\"t_objectproperties.Property\"/></Field></Table></RootTable></SrchOn><LnksTo/></Search></RootSearch>";

        private ISpecIfMetadataReader _metadataReader;

        public EaKafkaIntegrator(EAAPI.Repository repository, ISpecIfMetadataReader metadataReader)
        {
            _repository = repository;

            _metadataReader = metadataReader;

            _repository.CreateOutputTab("Kafka");

            _repository.EnsureOutputVisible("Kafka");
            
            AddSearches();

            StartKafkaEventListener();
        }

        private void StartKafkaEventListener()
        {
            _repository.WriteOutput("Kafka", "Listening to SpecIF events...", 0);

            RunEventListenerAsync();
        }

        private async void RunEventListenerAsync()
        {
            ConsumerConfig consumerConfiguration = new ConsumerConfig
            {
                GroupId = "ea-consumer-group",
                BootstrapServers = "localhost:9092",

                // Note: The AutoOffsetReset property determines the start offset in the event
                // there are not yet any committed offsets for the consumer group for the
                // topic/partitions of interest. By default, offsets are committed
                // automatically, so in this example, consumption will only start from the
                // earliest message in the topic 'my-topic' the first time you run the program.
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            IConsumer<Ignore, Resource> consumer = new ConsumerBuilder<Ignore, Resource>(consumerConfiguration)
                                                            .SetValueDeserializer(new KafkaJsonDeserializer<Resource>())
                                                            .Build();

            consumer.Subscribe("specif-events");

            while (true)
            {
                try
                {
                    ConsumeResult<Ignore, Resource> cr = consumer.Consume();
                    

                    //Debug.WriteLine("Receieved event");

                    Resource receivedEvent = cr.Message.Value;

                    if (receivedEvent != null)
                    {
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
                }
                catch (ConsumeException consumeException)
                {
                    Debug.WriteLine($"Error occured: {consumeException.Error.Reason}");
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

            if(searchResult.Count > 0)
            {
                EAAPI.Element eaElement = searchResult.GetAt(0) as EAAPI.Element;

                eaElement.SetTaggedValueString("specifId", changedResource.ID, false);

                eaElement.SetTaggedValueString("specifRevision", changedResource.Revision, false);

                string title = changedResource.GetPropertyValue("dcterms:title", _metadataReader);

                string description = changedResource.GetPropertyValue("dcterms:description", _metadataReader);

                eaElement.Name = title;

                eaElement.Notes = description;

                eaElement.Update();

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

            if(searchResult.Count > 0)
            {
                EAAPI.Element packageElement = searchResult.GetAt(0) as EAAPI.Element;

                EAAPI.Package package = _repository.GetPackageByGuid(packageElement.ElementGUID);

                string title = newResource.GetPropertyValue("dcterms:title", _metadataReader);

                string description = newResource.GetPropertyValue("dcterms:description", _metadataReader);

                string identifier = newResource.GetPropertyValue("dcterms:identifier", _metadataReader);

                EAAPI.Element eaRequirement = package.AddElement(title, "Requirement");

                eaRequirement.Notes = description;

                eaRequirement.Stereotype = "fmcreq";

                eaRequirement.Update();

                eaRequirement.SetTaggedValueString("specifId", newResource.ID, false);

                eaRequirement.SetTaggedValueString("specifRevision", newResource.Revision, false);

                eaRequirement.SetTaggedValueString("Identifier", identifier, false);

                _repository.AdviseElementChange(eaRequirement.ElementID);
            }
            else
            {
                // TODO Create packages
            }
        }


        private void AddSearches()
        {
            _repository.AddDefinedSearches(SPECIF_ELEMENT_SEARCH);
            _repository.AddDefinedSearches(JIRA_REQUIREMENTS_FOLDER_SEARCH);
        }
    }
}
