using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Fonlow.TraceHub.Security;
using System.Configuration;
using Microsoft.AspNet.SignalR.Hubs;

namespace Fonlow.TraceHub
{
    [System.CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TraceHubAuthorizeAttribute : AuthorizeAttribute
    {
        public TraceHubAuthorizeAttribute()
        {
            var s = System.Configuration.ConfigurationManager.AppSettings["loggingHub_Anonymous"];
            bool.TryParse(s, out allowAnonymous);
        }

        bool allowAnonymous = false;

        protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
        {
            if (allowAnonymous)
                return true;

            return base.UserAuthorized(user);
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            if (allowAnonymous)
                return true;

            return base.AuthorizeHubConnection(hubDescriptor, request);
        }
    }
}
