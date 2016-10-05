using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using WebApp_OpenIDConnect_DotNet_B2C.Policies;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net;

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

            return View();
        }

        public ActionResult Error(string message)
        {
            ViewBag.Message = message;

            return View("Error");
        }

        // Action to call Userinfo endpoint 
        public ActionResult Userinfo()
        {
            var identity = (ClaimsIdentity)ClaimsPrincipal.Current.Identity;

            if (identity != null && identity.IsAuthenticated)
            {
                string bid_code = identity.FindFirst("bid_code").Value;

                if (string.IsNullOrEmpty(bid_code))
                {
                    ViewBag.Message1 = "bid_code not found!";
                }
                else
                {
                    Helper h = new Helper();

                    JObject userinfo = h.CallUserinfoEndpoint(bid_code).Result;
                    ViewBag.Userinfo = userinfo;
                }

            }

            return View();
        }
    }

    // Simple helper class to call userinfo endpoint. 
    // Class should be moved in its entirety out of HomeController scope.
    public class Helper
    {
        // Should obviously be moved to app.config
        private string clientId = "DotNetClient";
        private string clientSecret = "1234";
        private string redirectUri = "https://localhost:44320/";
        private string OIDC_baseUrl = "https://preprod.bankidapis.no/oidc/oauth/";

        public Helper() { }

        /// <summary>
        /// Simple method to call the OIDC userinfo enpoint
        /// </summary>
        /// <param name="bidCode">The bid_code parameter to be passed along in GetToken()</param>
        /// <returns>A task containing JSON objects</returns>
        public async Task<JObject> CallUserinfoEndpoint(string bidCode)
        {
            // Get token
            string token = GetToken(bidCode).Result.ToString();
  
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, OIDC_baseUrl + "userinfo");
            
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Send message. Use ConfigureAwait(false) to circumvent deadlock
            var response = await (new HttpClient()).SendAsync(msg,HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            JObject tokenResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

            return tokenResponse;
        }

        /// <summary>
        /// Simple method to get an access token from the OIDC endpoint
        /// </summary>
        /// <param name="bidCode">The bid_code parameter to be passed on in the web-request</param>
        /// <returns>The access token, stringified</returns>
        public async Task<string> GetToken(string bidCode)
        {
            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, OIDC_baseUrl + "token");
            msg.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret)));
            msg.Content = new StringContent(" code=" + bidCode + "&redirect_uri=" + redirectUri + "&grant_type=authorization_code",
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded");

            // Send message. Use ConfigureAwait(false) to circumvent deadlock
            var response = await (new HttpClient()).SendAsync(msg,HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            JObject tokenResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

            var access_token = tokenResponse["access_token"].Value<string>() ?? string.Empty;

            return access_token;
        }
    }
}