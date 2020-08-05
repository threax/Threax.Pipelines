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

$os = [System.Environment]::OSVersion.Platform

if($os -eq [System.PlatformID]::Unix){
    $target = "ubuntu.18.04-x64"
}
else {
    $target = "win-x64"
}

Remove-Item -Recurse "$scriptPath/bin" -ErrorAction 'SilentlyContinue' # Have to call this every time or errors can occur

# Build the image then extract the tool from it by running it.
docker build --build-arg TARGET=$target "$srcDir" -f "$srcDir/Threax.DockerTools/Dockerfile" -t threax-docker-tools-builder
docker run -it --rm -v "${scriptpath}:/out" threax-docker-tools-builder