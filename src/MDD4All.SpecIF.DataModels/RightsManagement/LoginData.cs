using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels.RightsManagement
{
    public class LoginData
    {
        [JsonProperty(PropertyName = "username")]
        public string UserName = "";

        [JsonProperty(PropertyName = "password")]
        public string Password = "";
    }
}
