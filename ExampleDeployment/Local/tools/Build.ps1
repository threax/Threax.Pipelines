$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

docker build "$scriptPath/../../../" -f "$scriptPath/../../../Threax.DockerTools/Dockerfile" -t threax-docker-tools-builder # This builds the tools with docker
docker run -it --rm -v "${scriptpath}:/out" threax-docker-tools-builder # This run line extracts the compiled tools from the image
Remove-Item -Recurse "$scriptPath/bin"
Move-Item "$scriptPath/win" "$scriptPath/bin"
Remove-Item -Recurse "$scriptPath/ubuntu"