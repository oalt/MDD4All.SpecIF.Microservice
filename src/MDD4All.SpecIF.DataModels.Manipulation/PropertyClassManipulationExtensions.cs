using MDD4All.SpecIF.DataProvider.Contracts;

namespace MDD4All.SpecIF.DataModels.Manipulation
{
    public static class PropertyClassManipulationExtensions
    {
        public static string GetDataTypeTitle(this PropertyClass propertyClass, ISpecIfMetadataReader dataProvider)
        {
            string result = "";

            DataType dataType = dataProvider.GetDataTypeByKey(propertyClass.DataType);

            if (dataType != null)
            {
                result = dataType.Title.ToString();
            }

            return result;
        }
    }
}
