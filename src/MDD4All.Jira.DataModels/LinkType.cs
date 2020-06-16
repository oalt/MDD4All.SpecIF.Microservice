using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels
{
    public class LinkType
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("inward")]
        public string Inward { get; set; }

        [JsonProperty("outward")]
        public string Outward { get; set; }

        [JsonProperty("self")]
        public string Self { get; set; }
    }
}
