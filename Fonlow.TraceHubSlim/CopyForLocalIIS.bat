cd %~dp0
set target=C:\inetpub\wwwroot\TraceHubSlim\
xcopy bin\*.dll %target%bin\ /Y /D
xcopy Scripts\custom\logging.js %target%Scripts\custom\logging.js* /Y /D
xcopy Content\Site.css %target%Content\Site.css* /Y /D


copy Web.config %target%web.config /Y /D
copy index.html %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i Web.config /x C:\Release\TraceHub\settings.xml /o %target%web.config /e Test /clean

pause