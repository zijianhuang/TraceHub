using System;
using System.Web.Mvc;
using System.Web.Caching;
using System.Web;

namespace Fonlow.Web.Logging
{
    /// <summary>
    /// Auto versioning of custom javascript files.
    /// </summary>
    /// <remarks>http://stackoverflow.com/questions/2185872/force-browsers-to-get-latest-js-and-css-files-in-asp-net-application</remarks>
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString IncludeVersionedJs(this HtmlHelper helper, string filename)
        {
            string version = GetVersion(helper, filename);
            return MvcHtmlString.Create("<script type='text/javascript' src='" + filename + version + "'></script>");
        }

        static string GetVersion(this HtmlHelper helper, string filename)
        {
            var context = helper.ViewContext.RequestContext.HttpContext;

            if (context.Cache[filename] == null)
            {
                var physicalPath = context.Server.MapPath(filename);
                var version = "?v=" + new System.IO.FileInfo(physicalPath).LastWriteTime.ToString("yyyyMMddHHmmss");
                context.Cache.Add(physicalPath, version, null, DateTime.UtcNow.AddSeconds(60), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                context.Cache[filename] = version;
                //the cache item will stay there globally in the worker process, not matter what CacheItemPriority, as long as the cache is not full.
                //It seem that cache has an implicit policy that make small value stay there much lonnger than expected.
                // in contrast, larger object will be removed asap accourding to explicit policy.
                // In this case, the version will likely be the same during the life cycle of the worker process if the cache is not too busy.
                return version;
            }
            else
            {
                return context.Cache[filename] as string;
            }
        }

        public static string GetFileWithVersion(this HtmlHelper helper, string filename)
        {
            return filename + GetVersion(helper, filename);
        }
    }


}