using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCoreSso.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync("Cookies");

            return Redirect(returnUrl);
        }
    }
}
