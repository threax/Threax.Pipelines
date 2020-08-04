$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

docker build "$scriptPath/.." -f "$scriptPath/Dockerfile" -t threax-docker-tools-builder # This builds the tools with docker
docker run -it --rm -v "${pwd}/bin:/out" threax-docker-tools-builder # This run line extracts the compiled tools from the image