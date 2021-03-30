using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCoreSso.ViewModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreSso.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExternalController : ControllerBase
    {
        [Route("Challenge")]
        [HttpGet]
        public IActionResult Challenge([FromQuery] ChallengeViewModel challengeVm)
        {
            if (string.IsNullOrEmpty(challengeVm.ReturnUrl))
            {
                challengeVm.ReturnUrl = "";
            }

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            // If the project will redirect back to a mobile app, remove this code
            if (Url.IsLocalUrl(challengeVm.ReturnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", challengeVm.ReturnUrl },
                    { "scheme", challengeVm.Scheme },
                }
            };

            return Challenge(props, challengeVm.Scheme);

        }

        [Route("Callback")]
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }
            // retrieve claims of the external user
            var externalUser = result.Principal;
            if (externalUser == null)
            {   
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            var returnUrl = result.Properties.Items["returnUrl"] ?? "";

            // code to collect access token and id token issued by active directory
            var idToken = result.Properties.GetTokenValue("id_token");
            var accessToken = result.Properties.GetTokenValue("access_token");
            var username = result.Principal.Claims.Where(x => x.Type == "preferred_username").Select(x => x.Value).FirstOrDefault();

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync("Cookies");

            returnUrl += $"?at={accessToken}&un={username}&it={idToken}";
            return Redirect(returnUrl);
        }

        [Route("Logout")]
        [HttpGet]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            returnUrl = $"https://localhost:5001/external/PostLogoutCallback?returnUrl={returnUrl}";
            return Redirect($"https://login.microsoftonline.com/bc02ebd1-945f-4a18-a0ed-bb7be1a965cd/oauth2/logout?post_logout_redirect_uri={returnUrl}");
        }

        [Route("PostLogoutCallback")]
        [HttpGet]
        public async Task<IActionResult> PostLogoutCallback(string returnUrl)
        {
            return Redirect(returnUrl);
        }
    }
}
