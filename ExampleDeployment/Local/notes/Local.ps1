$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json; if($LASTEXITCODE -ne 0) {throw "Error during build."}
&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" exec $scriptPath/../id/appsettings.json AddFromMetadata `
    "$dirName" `
    --exec-load secret "$dirName/JwtAuth__ClientSecret" "$scriptPath/appsettings.json" "JwtAuth__ClientSecret" `
    --exec-load secret "$dirName/SharedClientCredentials__ClientSecret" "$scriptPath/appsettings.json" "SharedClientCredentials__ClientSecret"