using MDD4All.SpecIF.DataModels;
using System.Collections.Generic;

namespace MDD4All.SpecIF.DataProvider.EA.Converters.DataModels
{
    public class DiagramResource
    {
        public Key Key { get; set; }

        public string EaGUID { get; set; }

        public Resource Resource { get; set; }

        public List<Statement> ImplicitDiagramStatements { get; set; } = new List<Statement>();
    }
}
