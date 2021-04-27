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


            WordDocumentationGenerator wordDocumentationGenerator = new WordDocumentationGenerator();

            string wordDocumentation = wordDocumentationGenerator.GenerateVocabularyDocumentation(classDefinitionRoots);

            File.WriteAllText(@"D:\work\github\SpecIF\documentation\04_SpecIF_Domain_Classes_Word.md", wordDocumentation);


            GithibDocumentationGenerator documentationGenerator = new GithibDocumentationGenerator();

            string githubDocumentation = documentationGenerator.GenerateVocabularyDocumentation(classDefinitionRoots);

            File.WriteAllText(@"D:\work\github\SpecIF\documentation\04_SpecIF_Domain_Classes_Github.md", githubDocumentation);

        }

        static void Main(string[] args)
        {
            Program program = new Program();
        }
    }
}
