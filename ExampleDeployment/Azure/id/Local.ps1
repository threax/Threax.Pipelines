$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

Threax.DockerTools clone $scriptPath/appsettings.json
Threax.DockerTools build $scriptPath/appsettings.json
docker run --rm -v $scriptPath/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools "createCert signing 100 /out/id-server-signing-cert"
Threax.DockerTools run $scriptPath/appsettings.json