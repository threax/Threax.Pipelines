$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

docker build "$scriptPath/.." -f "$scriptPath/Dockerfile" -t threax/docker-tools