using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MDD4All.Jira.DataModels;
using MDD4All.SpecIF.DataAccess.Jira;
using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataModels.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MDD4All.SpecIF.Microservice.JiraWebhook.Controllers
{
    [Route("jira/webhook")]
    [ApiController]
    public class JiraWebhookController : ControllerBase
    {

        private IProducer<Null, Resource> _kafkaResourceProducer;

        private IConfiguration _configuration;


        public JiraWebhookController(IProducer<Null, Resource> kafkaResourceProducer,
                                     IConfiguration configuration)
        {
            _kafkaResourceProducer = kafkaResourceProducer;
            _configuration = configuration;
        }

        [HttpPost]
        public ActionResult Post([FromBody] JiraWebhookObject jiraWebhookObject)
        {

            ActionResult result = BadRequest();

            if (jiraWebhookObject != null && jiraWebhookObject.Issue != null)
            {
                string eventType = "V-SET-ResourceUpdated";

                if(jiraWebhookObject.WebhookEvent == "jira:issue_updated")
                {
                    eventType = "V-SET-ResourceUpdated";
                }
                else if(jiraWebhookObject.WebhookEvent == "jira:issue_created")
                {
                    eventType = "V-SET-ResourceCreated";
                }

                string resourceID = JiraGuidConverter.ConvertToSpecIfGuid(jiraWebhookObject.Issue.Self, jiraWebhookObject.Issue.ID);

                string resourceRevision = SpecIfGuidGenerator.ConvertDateToRevision(jiraWebhookObject.Issue.Fields.Updated.Value);

                int restIndex = jiraWebhookObject.Issue.Self.IndexOf("/rest");

                string origin = "Atlassian Jira " + jiraWebhookObject.Issue.Self.Substring(0, restIndex);

                string project = JiraGuidConverter.ConvertToSpecIfGuid(jiraWebhookObject.Issue.Self, jiraWebhookObject.Issue.Fields.Project.ID);

                string apiURL = _configuration.GetValue<string>("JiraSpecIfAPI"); 

                Key resourceKey = new Key(resourceID, resourceRevision);

                Key classKey = new Key("RC-Requirement", "1");

                Resource specIfEvent = CreateSpecIfEvent(origin,
                                                         project,
                                                         eventType,
                                                         apiURL,
                                                         resourceKey,
                                                         classKey);

                try
                {
                    _kafkaResourceProducer.Produce("specif-events", new Message<Null, Resource> { Value = specIfEvent });

                    result = new OkResult();
                }
                catch(Exception)
                {
                    result = BadRequest();
                }
            }

            return result;
        }

        private Resource CreateSpecIfEvent(string eventOrigin, 
                                           string project, 
                                           string eventType, 
                                           string apiURL,
                                           Key elementKey, 
                                           Key classKey)
        {
            Resource result = new Resource();

            result.Title = "Event from Jira.";

            result.Class = new Key("RC-SpecIfEvent", "1");

            Property eventSourceProperty = new Property("SpecIF:Origin",
                                                        new Key("PC-Origin", "1"),
                                                        eventOrigin,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(eventSourceProperty);

            Property projectProperty = new Property("SpecIF:project",
                                                        new Key("PC-SpecIfProject", "1"),
                                                        project,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(projectProperty);

            Property eventTypeProperty = new Property("SpecIF:specifEventType",
                                                        new Key("PC-SpecIfEventType", "1"),
                                                        eventType,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(eventTypeProperty);

            Property apiServerProperty = new Property("SpecIF:apiURL",
                                                        new Key("PC-ApiURL", "1"),
                                                        apiURL,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(apiServerProperty);

            Property idProperty = new Property("SpecIF:id",
                                                        new Key("PC-SpecIfId", "1"),
                                                        elementKey.ID,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(idProperty);

            Property revisionProperty = new Property("SpecIF:revision",
                                                        new Key("PC-SpecIfRevision", "1"),
                                                        elementKey.Revision,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(revisionProperty);

            Property classIdProperty = new Property("SpecIF:classId",
                                                        new Key("PC-SpecIfClassId", "1"),
                                                        classKey.ID,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(classIdProperty);

            Property classRevisionProperty = new Property("SpecIF:classRevision",
                                                        new Key("PC-SpecIfClassRevision", "1"),
                                                        classKey.Revision,
                                                        SpecIfGuidGenerator.CreateNewSpecIfGUID(),
                                                        DateTime.Now,
                                                        "Jira");

            result.Properties.Add(classRevisionProperty);

            return result;
        }
    }
}