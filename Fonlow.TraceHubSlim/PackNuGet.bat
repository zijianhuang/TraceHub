cd %~dp0
:not to use csproj but nuspec for not including other files. But then I have to define version in nuspec
nuget.exe pack Fonlow.TraceHub.Slim.nuspec -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause