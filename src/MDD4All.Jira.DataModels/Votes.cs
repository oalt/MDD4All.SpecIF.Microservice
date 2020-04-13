using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Votes
    {
        public string self { get; set; }
        public int votes { get; set; }
        public bool hasVoted { get; set; }
    }
}
