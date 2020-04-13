/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataModels.Service
{
	public interface ISpecIfServiceDescription
	{
		string ServiceName { get; set; }

		string ServiceAddress { get; set; }

		int ServicePort { get; set; }

		string ServiceDescription { get; set; }

		/// <summary>
		/// For example the address of the MongoDB server
		/// </summary>
		string ServiceNativeAccess { get; set; }

		string ServiceKind { get; set; }

		string IconURL { get; set; }

		string ID { get; set; }

		bool MetadataRead { get; set; }

		bool MetadataWrite { get; set; }

		bool DataRead { get; set; }

		bool DataWrite { get; set; }

        List<string> Tags { get; set; }
	}
}
