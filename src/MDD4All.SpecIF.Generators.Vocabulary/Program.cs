using MDD4All.SpecIF.DataProvider.File;
using System;
using System.IO;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    class Program
    {

        public Program()
        {
            string[] classDefinitionRoots = { @"d:\work\github\SpecIF\classDefinitions\1.1" };

            //VocabularyGenerator vocabularyGenerator = new VocabularyGenerator();

            //DataModels.SpecIF vocabulary = vocabularyGenerator.GenerateVocabulary(classDefinitionRoots);

            //SpecIfFileReaderWriter.SaveSpecIfToFile(vocabulary, @"c:\specif\GeneratedVocabulary.specif");

            DocumentationGenerator documentationGenerator = new DocumentationGenerator();

            string documentation = documentationGenerator.GenerateVocabularyDocumentation(classDefinitionRoots);

            File.WriteAllText(@"d:\specif\ClassesDocu.md", documentation);

        }

        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
