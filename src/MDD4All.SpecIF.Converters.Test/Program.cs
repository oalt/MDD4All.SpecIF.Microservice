/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.SpecIF.DataProvider.File;
using MDD4All.SpecIF.DataProvider.MongoDB;
using System;
using System.Collections.Generic;
using System.IO;

namespace MDD4All.SpecIF.Converters.Test
{
    class Program
    {
        static void Main(string[] args)
        {
			List<FileInfo> files = new List<FileInfo>();

			DirectoryInfo directoryInfo = new DirectoryInfo(@"c:\Users\olli\Documents\work\github\GfSESpecIF\classDefinitions");

			FileInfo[] baseFiles = directoryInfo.GetFiles("*.specif");

			files.AddRange(baseFiles);

			DirectoryInfo umlDirectoryInfo = new DirectoryInfo(@"c:\Users\olli\Documents\work\github\SpecIFSysML\classDefinitions");

			FileInfo[] umlFiles = umlDirectoryInfo.GetFiles("*.specif");

			files.AddRange(umlFiles);

			FileInfo umlData = new FileInfo(@"C:\Users\olli\Documents\work\github\SpecIF-Graph\source\specif\TestModel1.specif");

			files.Add(umlData);

			SpecIfMongoDbMetadataReader mongoDbMetadataReader = new SpecIfMongoDbMetadataReader("mongodb://localhost:27017");

			SpecIfMongoDbDataWriter mongoDbDataWriter = new SpecIfMongoDbDataWriter("mongodb://localhost:27017", mongoDbMetadataReader);
			SpecIfMongoDbMetadataWriter mongoDbMetadataWriter = new SpecIfMongoDbMetadataWriter("mongodb://localhost:27017");

			foreach (FileInfo file in files)
			{
				//FileToMongoDbConverter converter = new FileToMongoDbConverter(file.FullName, "mongodb://localhost:27017");
				//converter.ConvertFileToDB();

				DataModels.SpecIF specIF = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(file.FullName);

				if (specIF != null)
				{
					SpecIfConverter converter = new SpecIfConverter();
					converter.ConvertAll(specIF, mongoDbDataWriter, mongoDbMetadataWriter, true);
				}
			}
		}
    }
}
