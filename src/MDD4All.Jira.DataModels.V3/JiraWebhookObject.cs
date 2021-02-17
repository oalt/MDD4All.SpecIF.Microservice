/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDD4All.Jira.DataModels.V3
{
    public class JiraWebhookObject
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("webhookEvent")]
        public string WebhookEvent { get; set; }

        [JsonProperty("issue_event_type_name")]
        public string IssueEventTypeName { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("issue")]
        public Issue Issue { get; set; }

        [JsonProperty("changeLog")]
        public ChangeLog Changelog { get; set; }
        
    }

    //public class Customfield10018
    //{
    //    public bool hasEpicLinkFieldDependency { get; set; }
    //    public bool showField { get; set; }
    //    public NonEditableReason nonEditableReason { get; set; }
    //}


    
}
