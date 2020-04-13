using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Item
    {
        public string field { get; set; }
        public string fieldtype { get; set; }
        public string fieldId { get; set; }
        public object from { get; set; }
        public string fromString { get; set; }
        public object to { get; set; }
        public string toString { get; set; }
    }
}
