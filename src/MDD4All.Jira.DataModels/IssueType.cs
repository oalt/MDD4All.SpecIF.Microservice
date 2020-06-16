using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels
{
    public class IssueType
    {
        [JsonProperty("self")]
        public string Self { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconURL { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subtask")]
        public bool? Subtask { get; set; } = null;

        [JsonProperty("avatarId")]
        public int? AvatarID { get; set; } = null;
    }
   
}
