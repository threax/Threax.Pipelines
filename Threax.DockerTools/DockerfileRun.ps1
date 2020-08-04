$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

docker run -d --rm -v /var/run/docker.sock:/var/run/docker.sock --name threax-docker-tools threax/docker-tools tail -f /dev/null