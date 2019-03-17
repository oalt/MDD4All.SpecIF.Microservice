/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;

namespace MDD4All.SpecIF.DataModels.Helpers
{
    public class SpecIfGuidGenerator
    {
		public static string CreateNewSpecIfGUID()
		{
			string result = "";

			result = "_" + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "_");

			return result;
		}
	}
}
