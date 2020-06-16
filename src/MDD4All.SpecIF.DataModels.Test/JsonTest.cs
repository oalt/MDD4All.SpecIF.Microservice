using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels.Test
{
    public class JsonTest
    {
        public void Test1()
        {
            string json = System.IO.File.ReadAllText("./Testdata/json1.json");

            Resource resource = JsonConvert.DeserializeObject<Resource>(json, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            }
            );
        }
    }
}
