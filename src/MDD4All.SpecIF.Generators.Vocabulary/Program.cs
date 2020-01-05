using MDD4All.SpecIF.DataProvider.File;
using System;

namespace MDD4All.SpecIF.Generators.Vocabulary
{
    class Program
    {

        public Program()
        {
            VocabularyGenerator vocabularyGenerator = new VocabularyGenerator();

            string[] classDefinitionRoots = { @"c:\Users\olli\Documents\work\github\GfSESpecIF\classDefinitions" };

            DataModels.SpecIF vocabulary = vocabularyGenerator.GenerateVocabulary(classDefinitionRoots);

            SpecIfFileReaderWriter.SaveSpecIfToFile(vocabulary, @"c:\specif\GeneratedVocabulary.specif");

        }

        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
