using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System.Security.Claims;

namespace Fonlow.TraceHub.Security
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Thanks to http://blog.marcinbudny.com/2014/05/authentication-with-signalr-and-oauth.html#.V7KsnKLSNhE</remarks>
    public class ApplicationOAuthBearerAuthenticationProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            // try to find bearer token in a cookie 
            // (by default OAuthBearerAuthenticationHandler 
            // only checks Authorization header)
            var tokenCookie = context.OwinContext.Request.Cookies["BearerToken"];
            if (!string.IsNullOrEmpty(tokenCookie))
                context.Token = tokenCookie;
            return Task.FromResult<object>(null);
        }

    }

}
