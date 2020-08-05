$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/../tools/bin/Threax.DockerTools" clone $scriptPath/appsettings.json
&"$scriptPath/../tools/bin/Threax.DockerTools" build $scriptPath/appsettings.json
docker run --rm -v $scriptPath/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools "createCert signing 100 /out/id-server-signing-cert"
&"$scriptPath/../tools/bin/Threax.DockerTools" run $scriptPath/appsettings.json