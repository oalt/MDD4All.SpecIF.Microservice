using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    public class EaDateToRevisionConverter
    {
        public static string ConvertDateToRevision(DateTime date)
        {
            string result = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");

            SHA1Managed sha1 = new SHA1Managed();

            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(date.ToUniversalTime().ToString()));

            result = string.Concat(hash.Select(b => b.ToString("x2")));

            return result;
        }
    }
}
