cd %~dp0
nuget.exe pack Fonlow.TraceHub.Slim.nuspec -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause