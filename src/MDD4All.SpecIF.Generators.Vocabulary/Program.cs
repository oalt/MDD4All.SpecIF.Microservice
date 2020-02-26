using MDD4All.SpecIF.DataProvider.File;
using System;
using System.IO;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    class Program
    {

        public Program()
        {
            string[] classDefinitionRoots = { @"c:\Users\olli\Documents\work\github\GfSESpecIF\classDefinitions" };

            //VocabularyGenerator vocabularyGenerator = new VocabularyGenerator();

            //DataModels.SpecIF vocabulary = vocabularyGenerator.GenerateVocabulary(classDefinitionRoots);

            //SpecIfFileReaderWriter.SaveSpecIfToFile(vocabulary, @"c:\specif\GeneratedVocabulary.specif");

            DocumentationGenerator documentationGenerator = new DocumentationGenerator();

            string documentation = documentationGenerator.GenerateVocabularyDocumentation(classDefinitionRoots);

            File.WriteAllText(@"c:\specif\doc1.md", documentation);

        }

        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
