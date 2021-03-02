using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA.Converters.DataModels
{
    public class PortStateResource
    {
        public Key Key { get; set; }

        public Resource Resource { get; set; }

        public Dictionary<string, Statement> Statements { get; set; } = new Dictionary<string, Statement>();
    }
}
