$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/../tools/bin/Threax.DockerTools" setsecret $scriptPath/appsettings.json private-key $scriptPath/../cert/privkey1.pem
&"$scriptPath/../tools/bin/Threax.DockerTools" setsecret $scriptPath/appsettings.json public-key $scriptPath/../cert/fullchain1.pem

&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json