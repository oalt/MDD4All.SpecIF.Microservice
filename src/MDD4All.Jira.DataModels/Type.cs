using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Type
    {
        public string id { get; set; }
        public string name { get; set; }
        public string inward { get; set; }
        public string outward { get; set; }
        public string self { get; set; }
    }
}
