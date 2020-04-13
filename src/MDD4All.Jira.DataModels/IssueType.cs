using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class IssueType
    {
        public string self { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        public string description { get; set; }
        public string iconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public bool? subtask { get; set; } = null;
        public int? avatarId { get; set; } = null;
    }
   
}
