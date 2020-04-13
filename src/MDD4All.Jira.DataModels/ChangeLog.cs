using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class ChangeLog
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
