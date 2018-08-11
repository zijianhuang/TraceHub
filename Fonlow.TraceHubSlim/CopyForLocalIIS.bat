cd %~dp0
set target=C:\inetpub\wwwroot\TraceHubSlim\
xcopy bin\*.dll %target%bin\ /Y /D

xcopy Scripts\custom\logging.js %target%Scripts\custom\logging.js* /Y /D
xcopy Scripts\bootstrap.min.js %target%Scripts\ /Y /D
xcopy Scripts\jquery.signalR-2.3.0.min.js %target%Scripts\ /Y /D
xcopy Scripts\jquery-3.3.1.min.js %target%Scripts\ /Y /D
xcopy Scripts\popper.min.js %target%Scripts\ /Y /D

xcopy Content\*.css %target%Content\ /Y /D


copy Web.config %target%web.config /Y /D
copy index.html %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i Web.config /x C:\Release\TraceHub\settings.xml /o %target%web.config /e Test /clean
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i Index.html /x C:\Release\TraceHub\settings.xml /o %target%Index.html /e Test /clean

pause