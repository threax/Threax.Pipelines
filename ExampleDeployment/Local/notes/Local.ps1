$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json

&"$scriptPath/../tools/bin/Threax.DockerTools" createbase64secret "$scriptPath/appsettings.json" "SharedClientCredentials__ClientSecret" 32
&"$scriptPath/../tools/bin/Threax.DockerTools" createbase64secret "$scriptPath/appsettings.json" "JwtAuth__ClientSecret" 32

&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json