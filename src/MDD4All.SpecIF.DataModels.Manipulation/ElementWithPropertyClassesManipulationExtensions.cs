using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class ElementWithPropertyClassesManipulationExtensions
    {

        public static string GetPropertyClassTitle(this Key propertyClassKey,
                                                   ISpecIfMetadataReader dataProvider)
        {
            string result = "";

            result = dataProvider.GetPropertyClassByKey(propertyClassKey).Title.ToString();

            return result;
        }

       
    }
}
