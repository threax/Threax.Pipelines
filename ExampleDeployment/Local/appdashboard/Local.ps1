$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
$clientSecretName = "JwtAuth__ClientSecret"

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json

&"$scriptPath/../tools/bin/Threax.DockerTools" createbase64secret "$scriptPath/appsettings.json" $clientSecretName 32

&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json

# Load app metadata into id server
New-Item -ItemType Directory -Path "$scriptPath/../id/data/load/$dirName" -ErrorAction 'SilentlyContinue'
Copy-Item "$scriptPath/secrets/$clientSecretName" "$scriptPath/../id/data/load/$dirName/$clientSecretName"

&"$scriptPath/../tools/bin/Threax.DockerTools" exec "$scriptPath/../id/appsettings.json" SetupAppDashboard "/load/$dirName/$clientSecretName"

Remove-Item -Recurse "$scriptPath/../id/data/load/$dirName"