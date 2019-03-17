/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.IO;

namespace MDD4All.SpecIF.Converters.Test
{
    class Program
    {
        static void Main(string[] args)
        {
			DirectoryInfo directoryInfo = new DirectoryInfo(@"c:\Users\olli\Documents\work\github\GfSESpecIF\classDefinitions");

			FileInfo[] files = directoryInfo.GetFiles("*.specif");

			foreach(FileInfo file in files)
			{
				FileToMongoDbConverter converter = new FileToMongoDbConverter(file.FullName, "mongodb://localhost:27017");
				converter.ConvertFileToDB();
			}
		}
    }
}
