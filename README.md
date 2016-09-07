# TraceHub

This project delivers TraceHub, TraceHubSlim, HubTraceListener and TraceHub Console out of the box, as well as 3 NuGet packages for you to make your own Trace Hub in one of your existing ASP.NET Websites.

## Products out of the box

### TraceHub MVC
is an ASP.NET MVC Web application to provide centralized logging for Web and service applications through extending System.Diagnostics of .NET Framework. Traces from service applications could be displayed in TraceHub’s logging page.

A demo Website is available at http://tracehub.fonlow.com

### TraceHub Slim 
is single page ASP.NET Web application, containing only 1 HTML page, and no built-in authentication. 

A demo Website is available at http://tracehubslim.fonlow.com with demo traces pushed to your Web browsers.

### HubTraceListener 
is a derived class from TraceListenerBase of Essential.Diagnostics, while Essential.Diagnostics is an extension of System.Diagnostics, providing structured tracing and logging. Your .NET applications utilizing System.Diagnostics.Trace or TraceSource can utilize TraceHub through HubTraceListner without you altering one line of application codes, since trace listeners could be injected through app.config.

### TraceHub Console 
is a Windows console application that displays traces aggregated in TraceHub which will push traces to the Console when traces arrive. The Console is an alternative to TraceHub’s logging page, and also an interface for persisting traces.

## NuGet packages

#### [Fonlow.TraceHub.MVC](https://www.nuget.org/packages/Fonlow.TraceHub.MVC/)

#### [Fonlow.TraceHub.Slim](https://www.nuget.org/packages/Fonlow.TraceHub.Slim/)

#### [Fonlow.TraceHub.Core](https://www.nuget.org/packages/Fonlow.TraceHub.Core/)

#### [Fonlow.HubTraceListener](https://www.nuget.org/packages/Fonlow.HubTraceListener/)


And some demo ASP.NET Web projects utilizing these packages are available at https://github.com/tracehub/MyTraceHub


## Summary

TraceHub is fast and consuming very little system resources in all ends. Being injected with HubTraceListener, a service application sitting in the same zone with TraceHub could write 5000 lines of traces in 0.015 second, and it takes 0.36 second to push the traces from TraceHub to the Console or the Web browser. And this could happen when your service applications and TraceHub are located in Oregon, US, while the Console or the Web browser is in Brisbane, Australia.

Currently, the codebase is maintained at https://github.com/zijianhuang/TraceHub , the project management is carried out at https://www.fonlow.com:8443/redmine/projects/tracetoweb/issues . Please feel free to raise issues at the Issues section of Github.

Please check [Wiki](https://github.com/zijianhuang/TraceHub/wiki) for more details. And [this article published in CodeProject.com](http://www.codeproject.com/Articles/1118166/TraceHub-a-flexible-solution-for-Web-based-structu) explains some design concepts / contexts behind TraceHub.

