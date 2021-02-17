/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Item
    {
        [JsonProperty("field")]
        public string Field { get; set; }

        [JsonProperty("fieldtype")]
        public string Fieldtype { get; set; }

        [JsonProperty("fieldId")]
        public string FieldId { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("fromString")]
        public string FromString { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("toString")]
        public string ItemToString { get; set; }
    }
}
