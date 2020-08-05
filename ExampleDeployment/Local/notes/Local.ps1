$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
$clientSecretName = "JwtAuth__ClientSecret"
$clientCredsSecretName = "SharedClientCredentials__ClientSecret"

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json; if($LASTEXITCODE -ne 0) {throw "Error during build."}

&"$scriptPath/../tools/bin/Threax.DockerTools" createbase64secret "$scriptPath/appsettings.json" $clientSecretName 32
&"$scriptPath/../tools/bin/Threax.DockerTools" createbase64secret "$scriptPath/appsettings.json" $clientCredsSecretName 32

&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json

# Load app metadata into id server
New-Item -ItemType Directory -Path "$scriptPath/../id/data/load/$dirName" -ErrorAction 'SilentlyContinue'
Copy-Item "$scriptPath/secrets/$clientSecretName" "$scriptPath/../id/data/load/$dirName/$clientSecretName"
Copy-Item "$scriptPath/secrets/$clientCredsSecretName" "$scriptPath/../id/data/load/$dirName/$clientCredsSecretName"

&"$scriptPath/../tools/bin/Threax.DockerTools" exec "$scriptPath/../id/appsettings.json" AddFromMetadata https://notes.dev.threax.com "/load/$dirName/$clientSecretName" "/load/$dirName/$clientCredsSecretName"

Remove-Item -Recurse "$scriptPath/../id/data/load/$dirName"