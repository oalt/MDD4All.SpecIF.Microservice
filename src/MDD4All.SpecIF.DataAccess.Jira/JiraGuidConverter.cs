using MDD4All.SpecIF.DataModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MDD4All.SpecIF.DataAccess.Jira
{
    public class JiraGuidConverter
    {
        public static string ConvertToSpecIfGuid(string self, string issueKey)
        {

            string result = issueKey;

            int restIndex = self.IndexOf("/rest");

            string siteURL = self.Substring(0, restIndex);

            result = "_" + SpecIfGuidGenerator.CalculateSha1Hash(siteURL) + "_" + issueKey.Replace("-", "_");

            return result;
        }

        public static string GetIssueIdFromSpecIfID(string serverURL, string specIfID)
        {
            string result = "";

            string prefix = "_" + SpecIfGuidGenerator.CalculateSha1Hash(serverURL);

            if(specIfID.StartsWith(prefix))
            {
                result = specIfID.Replace(prefix, "").Replace("_", "-").Substring(1);
            }
            else
            {
                result = null;
            }

            return result;
        }

    }
}
