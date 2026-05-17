$ErrorActionPreference = 'Stop'
$livekitRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$exe = Join-Path $livekitRoot 'livekit-server.exe'

& $exe --dev --bind 127.0.0.1 --node-ip 127.0.0.1 --keys "studygroups_d85f55f2384eb63f: b6sIVu3PKuf2uNhBtfdGuEeT5h2bioS7nrytxbHWSZU"
