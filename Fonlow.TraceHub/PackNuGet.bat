cd %~dp0
nuget.exe pack Fonlow.TraceHub.nuspec -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause