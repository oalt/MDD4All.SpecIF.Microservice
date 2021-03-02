using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA.Converters.DataModels
{
    internal class ElementResource
    {
        public Key Key { get; set; }

        public string EaGUID { get; set; }

        public Resource Resource { get; set; }

        public List<Statement> ImplicitElementsStatements { get; set; } = new List<Statement>();
    }
}
