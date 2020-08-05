param (
    [Parameter(Position=0,mandatory=$true)]$destDir,
    [Parameter(Position=1)]$unixTarget = "ubuntu.18.04-x64"
)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

$os = [System.Environment]::OSVersion.Platform

if($os -eq [System.PlatformID]::Unix){
    $target = $unixTarget
}
else {
    $target = "win-x64"
}

# Build the image then extract the tool from it by running it.
docker build --build-arg TARGET=$target "$scriptPath/.." -f "$scriptPath/Dockerfile" -t threax-docker-tools-builder
docker run -it --rm -v "${destDir}:/out" threax-docker-tools-builder