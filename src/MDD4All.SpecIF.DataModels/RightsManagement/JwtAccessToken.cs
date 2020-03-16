using Newtonsoft.Json;

namespace MDD4All.SpecIF.DataModels.RightsManagement
{
    public class JwtAccessToken
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; } = "";

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; } = 0;
    }
}
