/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Newtonsoft.Json;
using System.IO;


namespace MDD4All.SpecIF.DataProvider.File
{
	public class SpecIfFileReaderWriter
	{
		public static DataModels.SpecIF ReadDataFromSpecIfFile(string path)
		{
			DataModels.SpecIF result;

			StreamReader file = new StreamReader(path);

			JsonSerializer serializer = new JsonSerializer();

			result = (DataModels.SpecIF)serializer.Deserialize(file, typeof(DataModels.SpecIF));

			file.Close();

			return result;
		}

		public static void SaveSpecIfToFile(DataModels.SpecIF data, string path)
		{
			StreamWriter sw = new StreamWriter(path);
			JsonWriter writer = new JsonTextWriter(sw)
			{
				Formatting = Formatting.Indented
			};

			JsonSerializer serializer = new JsonSerializer()
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			serializer.Serialize(writer, data);

			writer.Flush();
			writer.Close();

		}
	}
}
