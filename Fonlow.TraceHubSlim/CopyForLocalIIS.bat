cd %~dp0
set target=C:\inetpub\wwwroot\TraceHubSlim\
robocopy bin\ %target%bin\ /MIR
robocopy Scripts\ %target%Scripts\ /MIR
robocopy Content\ %target%Content\ /MIR
robocopy fonts\ %target%fonts\ /MIR


copy Web.config %target%web.config /Y /D
copy index.html %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i Web.config /x C:\Release\TraceHub\settings.xml /o %target%web.config /e Test /clean

pause