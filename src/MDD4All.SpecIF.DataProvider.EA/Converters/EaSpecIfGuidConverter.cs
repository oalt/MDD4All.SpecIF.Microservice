/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    public class EaSpecIfGuidConverter
    {
        public static string ConvertEaGuidToSpecIfGuid(string eaGuid)
        {
            string result = eaGuid.Replace('-', '_');

            result = result.Replace("{", "");
            result = result.Replace("}", "");
			result = "_" + result;

            return result;
        }

        public static string ConvertSpecIfGuidToEaGuid(string specIfGuid)
        {
			string result = "";

			if(specIfGuid.StartsWith("_"))
			{
				result = specIfGuid.Substring(1);
			}

			result = result.Replace('_', '-');

			result = "{" + result + "}";
            
            return result;
        }
    }
}
