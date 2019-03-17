/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MDD4All.SpecIF.DataModels.Test
{
	public class SpecIfTest1
	{
		public SpecIfTest1()
		{
			StreamReader file = File.OpenText(@"c:\specif\Test (schema 0-11-1) OK.specif");


			JsonSerializer serializer = new JsonSerializer();

			SpecIF specIf = (SpecIF) serializer.Deserialize(file, typeof(SpecIF));

			string vorname = specIf.CreatedBy.GivenName;

			using (StreamWriter outfile = File.CreateText(@"c:\specif\out2.specif"))
			{
				serializer = new JsonSerializer();
				serializer.Serialize(outfile, specIf);
			}

		}
	}
}
