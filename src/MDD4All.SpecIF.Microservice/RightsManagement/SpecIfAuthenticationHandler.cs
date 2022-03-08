using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MDD4All.SpecIF.DataModels.RightsManagement;
using Microsoft.Extensions.Primitives;

namespace MDD4All.SpecIF.Microservice.RightsManagement
{
    public class SpecIfAuthenticationHandler : AuthenticationHandler<SpecIfAuthenticationOptions>
    {

        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserRoleStore<ApplicationUser> _roleStore;

        public SpecIfAuthenticationHandler(IOptionsMonitor<SpecIfAuthenticationOptions> options,
                                           ILoggerFactory logger,
                                           UrlEncoder encoder,
                                           ISystemClock clock,
                                           IUserStore<ApplicationUser> userStore,
                                           IUserRoleStore<ApplicationUser> roleStore) : base(options, logger, encoder, clock)
        {
            _userStore = userStore;
            _roleStore = roleStore;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            AuthenticateResult result = AuthenticateResult.NoResult();

            Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out StringValues apiKeyHeaderValues);


            string providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey))
            {
                string metadataReadAuthRequired = Environment.GetEnvironmentVariable("metadataReadAuthRequired");
                if (metadataReadAuthRequired.ToLower().Equals("false"))
                {
                    ApplicationUser user = new ApplicationUser();
                    List<Claim> claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, "anonymousReader")
                        };
                    claims.Add(new Claim(ClaimTypes.Role, "anonReader"));
                    // claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, "anonReader")));
                    ClaimsIdentity identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                    List<ClaimsIdentity> identities = new List<ClaimsIdentity> { identity };
                    ClaimsPrincipal principal = new ClaimsPrincipal(identities);
                    AuthenticationTicket ticket = new AuthenticationTicket(principal, Options.Scheme);

                    result = AuthenticateResult.Success(ticket);
                }

            }
            else
            {
                ApplicationUser user = await ((SpecIfApiUserStore)_userStore).FindByApiKeyAsync(providedApiKey);

                if (user != null)
                {
                    List<Claim> claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.Name, user.NormalizedUserName)
                            };

                    claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                    ClaimsIdentity identity = new ClaimsIdentity(claims, Options.AuthenticationType);
                    List<ClaimsIdentity> identities = new List<ClaimsIdentity> { identity };
                    ClaimsPrincipal principal = new ClaimsPrincipal(identities);
                    AuthenticationTicket ticket = new AuthenticationTicket(principal, Options.Scheme);

                    result = AuthenticateResult.Success(ticket);

                }
            }


            return result;
        }

    }
}
