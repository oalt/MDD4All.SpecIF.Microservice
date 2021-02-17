/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Issue
    {
        [JsonProperty("expand")]
        public string Expand { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("names")]
        public Dictionary<string, string> FieldNames { get; set; } = new Dictionary<string, string>();

        [JsonProperty("changelog")]
        public ChangeLog ChangeLog { get; set; }

        [JsonIgnore]
        public Fields Fields { get; set; } = new Fields();
        
        [JsonProperty("fields")]
        public Dictionary<string, object> FieldDictionary
        {
            get
            {
                return Fields.FieldDictionary;
            }

            set
            {
                Fields = new Fields(value);
            }
        }
    }
}
