using MDD4All.Jira.DataModels;
using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataAccess.Jira
{
    public class SpecIfToJiraConverter
    {

        public string ConvertDescription(string xhtml)
        {
            var converter = new ReverseMarkdown.Converter();

            string result = converter.Convert(xhtml);

            return result;
        }

    }
}
