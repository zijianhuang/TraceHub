# TraceHub

This project delivers TraceHub, TraceHubSlim, HubTraceListener and TraceHub Console out of the box, as well as 3 NuGet packages for you to make your own Trace Hub in one of your existing ASP.NET Websites.

### TraceHub 
is to provide centralized logging for Web and service applications through extending System.Diagnostics of .NET Framework. Traces from service applications could be displayed in TraceHub’s logging page.

### TraceHubSlim 
is a slim version of TraceHub, containing only 1 Web page, and no built-in authentication. 

### HubTraceListener 
is a derived from TraceListenerBase of Essential.Diagnostics, while Essential.Diagnostics is an extension of System.Diagnostics, providing structured tracing and logging. Your .NET applications utilizing System.Diagnostics can utilize TraceHub through HubTraceListner without you altering one line of application codes, since trace listeners could be injected through app.config.

### TraceHub Console 
is a Windows console application that displays traces aggregated in TraceHub which will push traces to the Console when traces arrive. The Console is an alternative to TraceHub’s logging page.

TraceHub is fast and consuming very little system resources in all ends. Being injected with HubTraceListener, a service application sitting in the same zone with TraceHub could write 5000 lines of traces in 0.015 second, and it takes 0.36 second to push the traces from TraceHub to the Console or the Web browser. And this could happen when your service applications and TraceHub are located in Oregon, US, while the Console or the Web browser is in Brisbane, Australia.

Currently, the codebase is maintained at https://github.com/zijianhuang/TraceHub , the project management is carried out at https://www.fonlow.com:8443/redmine/projects/tracetoweb/issues . Please feel free to raise issues at the Issues section of Github.

Please check [Wiki](https://github.com/zijianhuang/TraceHub/wiki) for more details.

