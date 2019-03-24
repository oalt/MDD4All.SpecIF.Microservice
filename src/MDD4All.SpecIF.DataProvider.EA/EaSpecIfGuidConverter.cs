using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA
{
    public class EaSpecIfGuidConverter
    {
        public static string ConvertEaGuidToSpecIfGuid(string eaGuid)
        {
            string result = eaGuid.Replace('-', '_');

            result = result.Replace("{", "");
            result = result.Replace("}", "");

            return result;
        }

        public static string ConvertSpecIfGuidToEaGuid(string specIfGuid)
        {
            string result = specIfGuid.Replace('_', '-');

            result = "{" + result + "}";
            
            return result;
        }
    }
}
