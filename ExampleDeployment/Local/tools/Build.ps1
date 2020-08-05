$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

$toolsRepo = "https://github.com/threax/Threax.Pipelines.git"
$srcDir = "$scriptPath/src"
if(Test-Path $srcDir) {
    Push-Location $srcDir
    git pull
    Pop-Location
}
else {
    git clone $toolsRepo $srcDir
}

Remove-Item -Recurse "$scriptPath/ubuntu"
Remove-Item -Recurse "$scriptPath/win"
docker build "$srcDir" -f "$srcDir/Threax.DockerTools/Dockerfile" -t threax-docker-tools-builder # This builds the tools with docker
docker run -it --rm -v "${scriptpath}:/out" threax-docker-tools-builder # This run line extracts the compiled tools from the image
Remove-Item -Recurse "$scriptPath/bin"
Move-Item "$scriptPath/win" "$scriptPath/bin"
Remove-Item -Recurse "$scriptPath/ubuntu"