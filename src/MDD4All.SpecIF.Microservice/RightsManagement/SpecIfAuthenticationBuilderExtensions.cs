/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using Microsoft.AspNetCore.Authentication;
using System;

namespace MDD4All.SpecIF.Microservice.RightsManagement
{
    public static class SpecIfAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeySupport(this AuthenticationBuilder authenticationBuilder, Action<SpecIfAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<SpecIfAuthenticationOptions, SpecIfAuthenticationHandler>(SpecIfAuthenticationOptions.DefaultScheme, options);
        }
    }
}
