$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json