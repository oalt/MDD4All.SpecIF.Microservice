using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Watches
    {
        public string self { get; set; }
        public int watchCount { get; set; }
        public bool isWatching { get; set; }
    }
}
