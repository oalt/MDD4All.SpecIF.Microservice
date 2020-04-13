/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MDD4All.SpecIF.DataModels.Helpers
{
    public class SpecIfGuidGenerator
    {
		public static string CreateNewSpecIfGUID()
		{
			string result = "";

			result = "_" + Guid.NewGuid().ToString().Replace("{", "").Replace("}", "").Replace("-", "_");

			return result;
		}

        public static string CreateNewRevsionGUID()
        {
            string result = "";

            result = Guid.NewGuid().ToString().Replace("{", "").Replace("}", "");

            return result;
        }

        public static string CalculateSha1Hash(string data)
        {
            string result = "";

            SHA1Managed sha1 = new SHA1Managed();

            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));

            result = string.Concat(hash.Select(b => b.ToString("x2")));

            return result;
        }

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
