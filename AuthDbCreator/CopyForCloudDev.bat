cd %~dp0
set target=C:\Release\TraceHub\AuthDevCreatorCloudDev\
xcopy bin\Debug\*.* %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i app.config /x ..\settings.xml /o %target%AuthDbCreator.exe.config /e MySql /clean
pause