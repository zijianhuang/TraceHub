using System;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Fonlow.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Configuration;
using Microsoft.Extensions.Configuration;

namespace Fonlow.TraceHub
{
    ///// <summary>
    ///// Whether to authorize calls according to appSetting loggingHub_Anonymous
    ///// </summary>
    ///// <remarks>However, the host like IIS may still require authentication.</remarks>
    //[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    //public class TraceHubAuthorizeAttribute : Microsoft.AspNetCore.Authorization.AuthorizeAttribute
    //{
    //    public TraceHubAuthorizeAttribute(IConfiguration config)
    //    {
    //        var appSettings = config.GetSection("appSettings");
    //        var s = appSettings["loggingHub_Anonymous"];
    //        bool.TryParse(s, out allowAnonymous);
    //    }

    //    bool allowAnonymous = false;

    //    protected override bool UserAuthorized(System.Security.Principal.IPrincipal user)
    //    {
    //        if (allowAnonymous)
    //            return true;

    //        return base.UserAuthorized(user);
    //    }

    //    public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
    //    {
    //        if (allowAnonymous)
    //            return true;

    //        return base.AuthorizeHubConnection(hubDescriptor, request);
    //    }
    //}
}
