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
using MDD4All.SpecIf.Microservice.RightsManagement;

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

            if (!Request.Headers.TryGetValue(ApiKeyConstants.HeaderName, out var apiKeyHeaderValues))
            {
                result = AuthenticateResult.NoResult();
            }
            else
            {
                string providedApiKey = apiKeyHeaderValues.FirstOrDefault();

                if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey))
                {
                    result = AuthenticateResult.NoResult();
                }
                else
                {
                    ApplicationUser user = await ((SpecIfApiUserStore)_userStore).FindByApiKeyAsync(providedApiKey);

                    if (user != null)
                    {
                        var claims = new List<Claim>
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
            }

            return result;
        }

        //protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        //{
        //    Response.StatusCode = 401;
        //    Response.ContentType = ProblemDetailsContentType;
        //    var problemDetails = new UnauthorizedProblemDetails();

        //    await Response.WriteAsync(JsonSerializer.Serialize(problemDetails, DefaultJsonSerializerOptions.Options));
        //}

        //protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
        //{
        //    Response.StatusCode = 403;
        //    Response.ContentType = ProblemDetailsContentType;
        //    var problemDetails = new ForbiddenProblemDetails();

        //    await Response.WriteAsync(JsonSerializer.Serialize(problemDetails, DefaultJsonSerializerOptions.Options));
        //}
    }
}
