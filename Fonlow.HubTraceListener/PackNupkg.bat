cd %~dp0
nuget.exe pack Fonlow.HubTraceListener.csproj -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause