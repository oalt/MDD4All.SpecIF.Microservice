using MDD4All.SpecIF.DataModels;
using MDD4All.SpecIF.DataProvider.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MDD4All.SpecIF.Converters
{
    public class MetadataConverterForSpecIfOneRelease
    {
        public MetadataConverterForSpecIfOneRelease(FileInfo fileInfo)
        {


            //string targetFilename = fileInfo.DirectoryName + "/" + "1_" + fileInfo.Name;

            string targetFilename = fileInfo.FullName;


            DataModels.SpecIF sourceData = SpecIfFileReaderWriter.ReadDataFromSpecIfFile(fileInfo.FullName);

            if (sourceData.PropertyClasses != null)
            {
                foreach (PropertyClass propertyClass in sourceData.PropertyClasses)
                {
                    propertyClass.DataType.Revision = "1";
                }
            }

            if (sourceData.ResourceClasses != null)
            {
                foreach (ResourceClass resourceClass in sourceData.ResourceClasses)
                {
                    if(resourceClass.PropertyClasses != null)
                    {
                        foreach(Key propertyClass in resourceClass.PropertyClasses)
                        {
                            propertyClass.Revision = "1";
                        }
                    }
                }
            }

            if (sourceData.StatementClasses != null)
            {
                foreach (StatementClass statementClass in sourceData.StatementClasses)
                {
                    if (statementClass.PropertyClasses != null)
                    {
                        foreach (Key propertyClass in statementClass.PropertyClasses)
                        {
                            propertyClass.Revision = "1";
                        }
                    }
                }
            }

            SpecIfFileReaderWriter.SaveSpecIfToFile(sourceData, targetFilename);
        }


    }
}
