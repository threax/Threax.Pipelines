param (
    [Parameter(Position=0,mandatory=$true)]$destDir,
    [Parameter(Position=1)]$unixTarget = "linux-x64"
)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$env:DOCKER_BUILDKIT = 1

$os = [System.Environment]::OSVersion.Platform

if($os -eq [System.PlatformID]::Unix){
    $target = $unixTarget
}
else {
    $target = "win-x64"
}

Remove-Item -Recurse "$destDir/*" -ErrorAction 'SilentlyContinue' # Have to call this every time or errors can occur

# Build the image then extract the tool from it by running it.
docker build --build-arg TARGET=$target "$scriptPath/.." -f "$scriptPath/Dockerfile" -t threax-docker-tools-builder --progress=plain
docker run -it --rm -v "${destDir}:/out" threax-docker-tools-builder