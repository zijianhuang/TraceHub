cd %~dp0
set target=C:\inetpub\wwwroot\TraceHubSlim\
robocopy bin\ %target%bin\ /MIR
xcopy Scripts\jquery-3.1.0.min.js %target%Scripts\ /Y /D
xcopy Scripts\jquery.signalR-2.2.1.min.js %target%Scripts\ /Y /D
xcopy Scripts\custom\logging.js %target%Scripts\custom\ /Y /D
xcopy Content\*.min.css %target%Content\ /Y /D
xcopy Content\Site.css %target%Content\ /Y /D

copy Web.config %target%web.config /Y /D
copy index.html %target% /Y /D
pause