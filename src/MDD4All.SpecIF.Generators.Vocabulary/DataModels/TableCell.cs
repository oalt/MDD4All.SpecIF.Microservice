using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MDD4All.SpecIF.Generators.Vocabulary.DataModels
{
    public class TableCell
    {
        public TableCell()
        {

        }

        public TableCell(string singleLineContent)
        {
            Content.Add(singleLineContent);
        }

        public List<string> Content { get; set; } = new List<string>();

        public int MaximumWidth
        {
            get
            {
                int result = 0;

                foreach(string contentLine in Content)
                {
                    if(contentLine.Length > result)
                    {
                        result = contentLine.Length;
                    }
                }

                return result;
            }
        }

    }
}
