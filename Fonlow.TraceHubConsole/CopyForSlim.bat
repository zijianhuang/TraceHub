cd %~dp0
set target=C:\Release\TraceHub\TraceHubConsoleSlim\
xcopy bin\Release\*.* %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i app.config /x C:\Release\TraceHub\settings.xml /o %target%Fonlow.TraceHubConsole.exe.config /e Slim /Clean
pause