$backend = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory "$PSScriptRoot\LMS_API" -PassThru -NoNewWindow
$frontend = Start-Process -FilePath "cmd.exe" -ArgumentList "/c npm run dev" -WorkingDirectory "$PSScriptRoot\lms_frontend" -PassThru -NoNewWindow

Write-Host "Backend PID: $($backend.Id)  |  Frontend PID: $($frontend.Id)"
Write-Host "Press Ctrl+C to stop both."

try {
    Wait-Process -Id $backend.Id, $frontend.Id
} finally {
    if (!$backend.HasExited)  { Stop-Process -Id $backend.Id  -Force }
    if (!$frontend.HasExited) { Stop-Process -Id $frontend.Id -Force }
    Write-Host "Stopped."
}
