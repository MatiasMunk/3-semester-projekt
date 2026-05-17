Get-CimInstance Win32_Process |
    Where-Object { $_.Name -eq 'livekit-server.exe' } |
    ForEach-Object {
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
        "Stopped livekit-server.exe PID $($_.ProcessId)"
    }
