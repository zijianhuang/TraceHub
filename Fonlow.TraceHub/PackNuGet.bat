cd %~dp0
nuget.exe pack Fonlow.TraceHub.MVC.nuspec -Prop Configuration=Release -OutputDirectory c:\release\TraceHub
pause