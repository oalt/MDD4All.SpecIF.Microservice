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

        private const string CONNECTION_STRING = "mongodb://localhost:27017";

        //private const string CONNECTION_STRING = "mongodb+srv://admin:2IkHZ77ZuuP0e6NN@specifcluster-ausx9.azure.mongodb.net/test?retryWrites=true";


        static void Main(string[] args)
        {
			List<FileInfo> files = new List<FileInfo>();

			DirectoryInfo directoryInfo = new DirectoryInfo(@"c:\Users\olli\Documents\work\github\GfSESpecIF\classDefinitions");

            foreach(DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories())
            {
                FileInfo[] baseFiles = subDirectoryInfo.GetFiles("*.specif");

                //files.AddRange(baseFiles);
            }

			

			DirectoryInfo umlDirectoryInfo = new DirectoryInfo(@"c:\Users\olli\Documents\work\github\SpecIFSysML\classDefinitions");

			FileInfo[] umlFiles = umlDirectoryInfo.GetFiles("*.specif");

			//files.AddRange(umlFiles);

			FileInfo umlData = new FileInfo(@"C:\Users\olli\Documents\work\github\SpecIF-Graph\source\specif\TestModel1.specif");

            //files.Add(umlData);

            FileInfo vocabularyData = new FileInfo(@"c:\specif\GeneratedVocabulary.specif");
            files.Add(vocabularyData);

			SpecIfMongoDbMetadataReader mongoDbMetadataReader = new SpecIfMongoDbMetadataReader(CONNECTION_STRING);
            SpecIfMongoDbDataReader mongoDbDataReader = new SpecIfMongoDbDataReader(CONNECTION_STRING);
            SpecIfMongoDbDataWriter mongoDbDataWriter = new SpecIfMongoDbDataWriter(CONNECTION_STRING, mongoDbMetadataReader, mongoDbDataReader);
			SpecIfMongoDbMetadataWriter mongoDbMetadataWriter = new SpecIfMongoDbMetadataWriter(CONNECTION_STRING);

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
