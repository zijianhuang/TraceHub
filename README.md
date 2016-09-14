# TraceHub

TraceHub Service optionally accompanied with MVC Web UI or Slim Web UI is to provide centralized logging for Web and service applications through extending System.Diagnostics of .NET Framework. 

This project delivers TraceHub MVC, TraceHub Slim, HubTraceListener and TraceHub Console out of the box. 

**Key features**

* Centralized and distributed logging for Web applications and services
* Extending System.Diagnostics and Essential.Diagnostics
* Providing structured tracing and logging withou needing to change 1 line of your application codes

## Products

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


## Summary

TraceHub requires no change in your application codes, and is fast and consuming very little system resources in all ends. Being injected with HubTraceListener, a service application sitting in the same zone with TraceHub could write 5000 lines of traces in 0.015 second, and it takes 0.36 second to push the traces from TraceHub to the Console or the Web browser. And this could happen when your service applications and TraceHub are located in Oregon, US, while the Console or the Web browser is in Brisbane, Australia.

Currently, the codebase is maintained at https://github.com/zijianhuang/TraceHub , the project management is carried out at https://www.fonlow.com:8443/redmine/projects/tracetoweb/issues . Please feel free to raise issues at the Issues section of Github.

Please check [Wiki](https://github.com/zijianhuang/TraceHub/wiki) for more details. And [this article published in CodeProject.com](http://www.codeproject.com/Articles/1118166/TraceHub-a-flexible-solution-for-Web-based-structu) explains some design concepts / contexts behind TraceHub.

## Additional Resources that could be used along with TraceHub and Your ASP.NET Applications

* [RollingFileTraceListener](https://essentialdiagnostics.codeplex.com/wikipage?title=RollingFileTraceListener)
* [RollingXmlTraceListener](https://essentialdiagnostics.codeplex.com/wikipage?title=RollingXmlTraceListener)
* [SqlDatabaseTraceListener](https://essentialdiagnostics.codeplex.com/wikipage?title=SqlDatabaseTraceListener)
* [BufferedEmailTraceListener](https://essentialdiagnostics.codeplex.com/wikipage?title=BufferedEmailTraceListener)
* [EmailTraceListener](https://essentialdiagnostics.codeplex.com/wikipage?title=EmailTraceListener)
* [ElasticSearchTraceListener](https://github.com/amccool/ElasticSearch.Diagnostics)
* [AWS DynamoDB TraceListener](https://github.com/aws/aws-dotnet-trace-listener/)
* [DiagnosticMonitorTraceListener](https://azure.microsoft.com/en-us/documentation/articles/cloud-services-dotnet-diagnostics-trace-flow/)
