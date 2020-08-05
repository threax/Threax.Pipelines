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

Remove-Item -Recurse "$scriptPath/bin" -ErrorAction 'SilentlyContinue' # Have to call this every time or errors can occur

&"$srcDir/Threax.DockerTools/Build.ps1" $scriptPath