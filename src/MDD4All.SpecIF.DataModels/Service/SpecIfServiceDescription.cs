/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels.Service
{
	public class SpecIfServiceDescription : ISpecIfServiceDescription
	{
		public string ServiceName { get; set; }

        [JsonIgnore]
		public string ServiceAddress { get; set; }

        [JsonIgnore]
        public int ServicePort { get; set; }

		public string ServiceDescription { get; set; }

        public string IconURL { get; set; }

        public string ID { get; set; }

		public bool MetadataRead { get; set; }

		public bool MetadataWrite { get; set; }

		public bool DataRead { get; set; }

		public bool DataWrite { get; set; }

		public string ServiceNativeAccess { get; set; }

		public string ServiceKind { get; set; }

        public List<string> Tags { get; set; } = new List<string> { "SpecIF-API" };
    }
}
