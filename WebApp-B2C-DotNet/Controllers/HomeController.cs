using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebApp_OpenIDConnect_DotNet_B2C.Policies;

namespace WebApp_OpenIDConnect_DotNet_B2C.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // You can use the PolicyAuthorize decorator to execute a certain policy if the user is not already signed into the app.
        [PolicyAuthorize(Policy = "b2c_1_susi")]
        public ActionResult Claims()
        {
            Claim displayName = ClaimsPrincipal.Current.FindFirst(ClaimsPrincipal.Current.Identities.First().NameClaimType);
            ViewBag.DisplayName = displayName != null ? displayName.Value : string.Empty;
            /*
            ClaimsIdentity identity = ClaimsPrincipal.Current.Identities.First();

            if (identity != null && identity.IsAuthenticated)
            {
                string accessToken = identity.FindFirst("access_token").Value;

                if (string.IsNullOrEmpty(accessToken))
                {
                    ViewBag.Message1 = "no access token received!";
                }
                else
                {
                    ViewBag.Message1 = "Would call userinfo here!";
                }

            }
            */

            return View();
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message;

            return View("Error");
        }
    }
}