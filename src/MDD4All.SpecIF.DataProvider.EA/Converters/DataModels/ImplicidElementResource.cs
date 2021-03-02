using MDD4All.SpecIF.DataModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA.Converters.DataModels
{
    internal class ImplicitElementResource
    {
        public Key Key { get; set; }

        public Resource Resource { get; set; }

        public Key Parent { get; set; }

        public string ParentEaGUID { get; set; }

        public List<Statement> ImplicitElementsStatements { get; set; } = new List<Statement>();
    }
}
