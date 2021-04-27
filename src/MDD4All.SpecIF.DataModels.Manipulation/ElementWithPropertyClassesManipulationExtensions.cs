using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ElementWithPropertyClassesManipulationExtensions
    {

        public static string GetPropertyClassTitle(this Key propertyClassKey,
                                                   ISpecIfMetadataReader dataProvider)
        {
            string result = "";

            PropertyClass propertyClass = dataProvider.GetPropertyClassByKey(propertyClassKey);

            
            result = propertyClass.Title;
            

            return result;
        }

       
    }
}
