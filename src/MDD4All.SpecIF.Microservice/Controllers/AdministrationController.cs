/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MDD4All.SpecIf.Microservice.RightsManagement;
using MDD4All.SpecIF.DataModels.RightsManagement;
using MDD4All.SpecIF.DataProvider.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MDD4All.SpecIf.Microservice.Controllers
{
    /// <summary>
    /// Unofficial administration endpoint for the SpecIF Microsoervice.
    /// </summary>
    [ApiVersion("1.1")]
    [Produces("application/json")]
    [Route("specif/v{version:apiVersion}")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdministrationController : Controller
    {
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserRoleStore<ApplicationUser> _roleStore;

        private readonly IJwtConfigurationReader _jwtConfigurationReader;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userStore"></param>
        /// <param name="roleStore"></param>
        /// <param name="jwtConfigurationReader"></param>
        public AdministrationController(IUserStore<ApplicationUser> userStore,
                               IUserRoleStore<ApplicationUser> roleStore,
                               IJwtConfigurationReader jwtConfigurationReader)
        {
            _userStore = userStore;
            _roleStore = roleStore;
            _jwtConfigurationReader = jwtConfigurationReader;
        }

        /// <summary>
        /// Returns a Jwt token to access some SpecIF API endpoints.
        /// </summary>
        /// <param name="loginData">The user login data.</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("oauth/token")]
        [ProducesResponseType(typeof(JwtAccessToken), 200)]
        public async Task<ActionResult> GetJwtToken([FromBody]LoginData loginData)
        { 
            ActionResult result = new UnauthorizedResult();

            ApplicationUser checkUser = await CheckUser(loginData);

            if (checkUser != null)
            {
                object tokenObject = await GenerateToken(checkUser);
                
                result = new OkObjectResult(tokenObject);
            }

            return result;
        }
    
        /// <summary>
        /// Returns the list of registred users.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<ApplicationUser>), 200)]
        public async Task<ActionResult<List<ApplicationUser>>> GetUsers()
        {
            ActionResult result = NotFound();

            List<ApplicationUser> users = new List<ApplicationUser>();

            if(_userStore is SpecIfApiUserStore)
            {
                users = await ((SpecIfApiUserStore)_userStore).GetAllUsers();
                result = new OkObjectResult(users);
            }


            return result;
        }

        /// <summary>
        /// Add a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("users")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> AddUser([FromBody] LoginData user)
        {
            ActionResult result = BadRequest();

            if(user != null)
            {
                UpperInvariantLookupNormalizer lookupNormalizer = new UpperInvariantLookupNormalizer();
                PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = user.UserName,
                    NormalizedUserName = lookupNormalizer.NormalizeName(user.UserName),
                    Roles = new List<string>()
                };

                applicationUser.PasswordHash = passwordHasher.HashPassword(applicationUser, user.Password);

                if(await _userStore.FindByNameAsync(applicationUser.NormalizedUserName, CancellationToken.None) == null)
                {
                    await _userStore.CreateAsync(applicationUser, CancellationToken.None);
                    result = new OkResult();
                }
                

            }

            return result;
        }

        /// <summary>
        /// Delete an existing user.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("users")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> DeleteUser(string userID)
        {
            ActionResult result = BadRequest();

            if (!string.IsNullOrEmpty(userID))
            {
                ApplicationUser user = await _userStore.FindByIdAsync(userID, CancellationToken.None);
                if (user != null)
                {
                    await _userStore.DeleteAsync(user, CancellationToken.None);
                    result = new OkResult();
                }
                else
                {
                    result = NotFound();
                }
            }

            return result;
        }

        /// <summary>
        /// Update the roles of a user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut("users/{id}/roles")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> UpdateRoles(string id, [FromBody] List<String> roles)
        {
            ActionResult result = BadRequest();

            if (!string.IsNullOrEmpty(id) && roles != null)
            {
                ApplicationUser user = await _userStore.FindByIdAsync(id, CancellationToken.None);
                if (user != null)
                {
                    user.Roles = new List<string>();
                    foreach (string role in roles)
                    {
                        user.Roles.Add(role);
                    }

                    await _userStore.UpdateAsync(user, CancellationToken.None);

                    result = new OkResult();
                }
                else
                {
                    result = NotFound();
                }
            }

            return result;
        }


        

        /// <summary>
        /// Change password. This endpoint is usable by all authenticated users.
        /// </summary>
        /// <param name="userLogin"></param>
        /// <returns></returns>
        [Authorize()]
        [HttpPut("users/password")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> ChangePassword([FromBody] LoginData userLogin)
        {
            ActionResult result = BadRequest();

            ApplicationUser user = null;

            if (userLogin != null)
            {
                UpperInvariantLookupNormalizer lookupNormalizer = new UpperInvariantLookupNormalizer();

                string normalizedUserName = lookupNormalizer.NormalizeName(userLogin.UserName);


                ClaimsPrincipal currentUser = HttpContext.User;
                                

                if (currentUser.HasClaim(claim => claim.Type == ClaimTypes.Name))
                {
                    string normalizedCurrentUserName = currentUser.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name).Value;

                    ApplicationUser currentApplicationUser = await _userStore.FindByNameAsync(normalizedCurrentUserName, 
                                                                                   CancellationToken.None);

                    
                    if(currentApplicationUser.NormalizedUserName == normalizedUserName)
                    {
                        user = currentApplicationUser;
                    }
                    else
                    {
                        if(currentUser.HasClaim(roleClaim => roleClaim.Type == ClaimTypes.Role))
                        {
                            bool isAdmin = await _roleStore.IsInRoleAsync(currentApplicationUser, "Administrator", CancellationToken.None);
                            
                            if(isAdmin)
                            {
                                user = await _userStore.FindByNameAsync(normalizedUserName, CancellationToken.None);
                            }
                        }
                    }

                }
                
                if (user != null)
                {
                    PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
                    user.PasswordHash = passwordHasher.HashPassword(user, userLogin.Password);

                    await _userStore.UpdateAsync(user, CancellationToken.None);

                    result = new OkResult();
                }
                else
                {
                    result = NotFound();
                }
                
            }

            return result;
        }

        private async Task<ApplicationUser> CheckUser(LoginData loginData)
        {
            // should check in the database

            ApplicationUser result = null;

            UpperInvariantLookupNormalizer lookupNormalizer = new UpperInvariantLookupNormalizer();

            ApplicationUser user = await _userStore.FindByNameAsync(lookupNormalizer.NormalizeName(loginData.UserName), 
                                                                    CancellationToken.None);

            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();

            if (user != null)
            {
                PasswordVerificationResult passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginData.Password);
                if(passwordResult == PasswordVerificationResult.Success)
                {
                    result = user;
                }
            }

            return result;
        }

        private async Task<JwtAccessToken> GenerateToken(ApplicationUser user, int expireMinutes = 480)
        {

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfigurationReader.GetSecret()));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);



            IList<string> roles = await _roleStore.GetRolesAsync(user, CancellationToken.None);

            List<Claim> claims = new List<Claim>();

            foreach(string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            claims.Add(new Claim(ClaimTypes.Name, user.NormalizedUserName));

            var token = new JwtSecurityToken(_jwtConfigurationReader.GetIssuer(),
              _jwtConfigurationReader.GetIssuer(),
              claims,
              expires: DateTime.Now.AddMinutes(expireMinutes),
              signingCredentials: credentials);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            JwtAccessToken result = new JwtAccessToken()
            {
                AccessToken = tokenString,
                TokenType = "Bearer",
                ExpiresIn = (expireMinutes * 60)
            };

            return result;
        }
    }
}