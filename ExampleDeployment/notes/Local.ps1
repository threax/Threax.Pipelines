$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

Threax.DockerTools clone $scriptPath/appsettings.json
Threax.DockerTools build $scriptPath/appsettings.json
Threax.DockerTools run $scriptPath/appsettings.json