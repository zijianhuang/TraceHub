cd %~dp0
nuget.exe pack Fonlow.TraceHub.Core.csproj -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause