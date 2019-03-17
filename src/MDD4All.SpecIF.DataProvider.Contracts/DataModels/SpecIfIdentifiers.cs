/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.Contracts.DataModels
{
    public class SpecIfIdentifiers
    {
		public SpecIfIdentifiers()
		{
			Identifiers = new List<SpecIfIdentifier>();
		}

		public List<SpecIfIdentifier> Identifiers { get; set; }
    }
}
