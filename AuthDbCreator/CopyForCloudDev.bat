cd %~dp0
set target=C:\Release\TraceHub\AuthDevCreatorCloudDev\
xcopy bin\Release\*.* %target% /Y /D
c:\green\XmlPreprocess\bin\XmlPreprocess.exe /i app.config /x C:\Release\TraceHub\settings.xml /o %target%AuthDbCreator.exe.config /e MySql /clean
pause