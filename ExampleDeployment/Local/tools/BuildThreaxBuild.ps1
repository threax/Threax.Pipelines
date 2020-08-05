$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

# This is good to build, but it will be overwritten by whatever is on docker hub as the latest version.

$toolsRepo = "https://github.com/threax/build-threax.git"
$srcDir = "$scriptPath/src/build-threax"
if(Test-Path $srcDir) {
    Push-Location $srcDir
    git pull
    Pop-Location
}
else {
    git clone $toolsRepo $srcDir
}

&"$srcDir/Build.ps1" $scriptPath