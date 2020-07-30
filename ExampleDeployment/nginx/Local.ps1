$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

Threax.DockerTools setsecret $scriptPath/appsettings.json private-key $scriptPath/../cert/privkey1.pem
Threax.DockerTools setsecret $scriptPath/appsettings.json public-key $scriptPath/../cert/fullchain1.pem

Threax.DockerTools build $scriptPath/appsettings.json
Threax.DockerTools run $scriptPath/appsettings.json