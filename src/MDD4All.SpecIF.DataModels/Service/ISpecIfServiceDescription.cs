/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
namespace MDD4All.SpecIF.DataModels.Service
{
	public interface ISpecIfServiceDescription
	{
		string ServiceName { get; set; }

		string ServiceAddress { get; set; }

		int ServicePort { get; set; }

		string ServiceDescription { get; set; }

		string IconURL { get; set; }

		string ID { get; set; }

		bool MetadataRead { get; set; }

		bool MetadataWrite { get; set; }

		bool DataRead { get; set; }

		bool DataWrite { get; set; }
	}
}
