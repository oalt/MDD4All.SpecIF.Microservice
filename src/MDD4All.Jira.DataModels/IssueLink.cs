using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Issuelink
    {
        public string id { get; set; }
        public string self { get; set; }
        public Type type { get; set; }
        public InwardIssue inwardIssue { get; set; }
    }
}
