/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EAAPI = EA;

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

            if (specIfGuid.StartsWith("_"))
            {
                result = specIfGuid.Substring(1);
            }

            result = result.Replace('_', '-');

            result = "{" + result + "}";

            return result;
        }

        public static string CreatePortStateID(EAAPI.Element port1, EAAPI.Element port2)
        {
            string result = "";

            string idString = "";

            if(port1.ElementID < port2.ElementID)
            {
                idString = port1.ElementGUID + port2.ElementGUID;
            }
            else
            {
                idString = port2.ElementGUID + port1.ElementGUID;
            }

            SHA1Managed sha1 = new SHA1Managed();

            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(idString));

            result = string.Concat(hash.Select(b => b.ToString("x2")));

            result = "_" + result;

            return result;
        }
    }
}
