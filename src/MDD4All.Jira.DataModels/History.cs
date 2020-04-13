using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class History
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("author")]
        public User Author { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
