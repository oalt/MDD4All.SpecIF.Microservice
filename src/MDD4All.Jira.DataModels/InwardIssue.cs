using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class InwardIssue
    {
        public string id { get; set; }
        public string key { get; set; }
        public string self { get; set; }
        public Fields fields { get; set; }
    }
}
