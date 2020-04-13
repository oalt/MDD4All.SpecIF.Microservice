using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EAAPI = EA;

namespace MDD4All.SpecIF.Apps.EaIntegration
{
    public class EnterpriseArchitectStarter
    {

        public EAAPI.Repository Start(string fileName)
        {
            EAAPI.Repository result = new EAAPI.Repository();
           
            bool openResult = result.OpenFile(fileName);

            if (!openResult)
            {
                result = null;
            }

            result.ShowWindow(1);

            return result;
        }
    }
}
