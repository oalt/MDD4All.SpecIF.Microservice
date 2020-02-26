using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.EA.Converters;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EAAPI = EA;

namespace MDD4All.SoecIF.DataProvider.EA.Test
{
	public class TestEaSpecIfConverter
	{

		

		public void StartTest()
		{
			

			EAAPI.Repository repository = new EAAPI.Repository();

			try
			{
				string fileName = "TestModel1.eap";

				bool openResult = repository.OpenFile(AssemblyDirectory + "/TestData/" + fileName);

				// root package guid: {45A143E0-D43A-4f51-ACA6-FF695EEE3256}

				if (openResult)
				{
					Console.WriteLine("Model open");

					EAAPI.Package rootModelPackage = repository.GetPackageByGuid("{45A143E0-D43A-4f51-ACA6-FF695EEE3256}");

					SpecIF.DataModels.SpecIF specIF;

					//EaUmlToSpecIfConverter converter;

					//converter = new EaUmlToSpecIfConverter(repository);

					//specIF = converter.ConvertModelToSpecIF(rootModelPackage);
						

					//SpecIfFileReaderWriter.SaveSpecIfToFile(specIF, @"C:\Users\olli\Documents\work\github\SpecIF-Graph\source\specif\TestModel1.specif");

					Console.WriteLine("Finished");
				}
			}
			catch(Exception exception)
			{
				Console.WriteLine(exception);
			}
			finally
			{
				Console.ReadLine();
				repository.Exit();
			}

		}

		

		public static string AssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().CodeBase;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}
	}
}
