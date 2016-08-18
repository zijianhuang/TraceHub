cd %~dp0
set target=C:\Release\TraceHub\TraceHubConsoleTest\
xcopy bin\Release\*.* %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i app.config /x ..\settings.xml /o %target%Fonlow.TraceHubConsole.exe.config /e Test /clean
pause