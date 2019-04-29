/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
namespace MDD4All.SpecIF.DataModels.Service
{
	public class SpecIfServiceDescription : ISpecIfServiceDescription
	{
		public string ServiceName { get; set; }

		public string ServiceAddress { get; set; }

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
	}
}
