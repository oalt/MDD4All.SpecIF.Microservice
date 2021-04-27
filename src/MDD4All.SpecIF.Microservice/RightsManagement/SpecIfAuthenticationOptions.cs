/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Authentication;

namespace MDD4All.SpecIF.Microservice.RightsManagement
{
    public class SpecIfAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";
        public string Scheme => DefaultScheme;
        public string AuthenticationType = DefaultScheme;
    }
}
