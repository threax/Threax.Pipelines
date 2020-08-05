$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

$toolsRepo = "https://github.com/threax/Threax.Pipelines.git"
$srcDir = "$scriptPath/src/Threax.Pipelines"
if(Test-Path $srcDir) {
    Push-Location $srcDir
    git pull
    Pop-Location
}
else {
    git clone $toolsRepo $srcDir
}

&"$srcDir/Threax.DockerTools/Build.ps1" $scriptPath