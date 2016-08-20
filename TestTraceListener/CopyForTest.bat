cd %~dp0
set target=C:\Release\TraceHub\TestTraceListenerTest\
xcopy bin\Release\*.* %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i app.config /x C:\Release\TraceHub\settings.xml /o %target%TestTraceListener.exe.config /e Test /clean
pause