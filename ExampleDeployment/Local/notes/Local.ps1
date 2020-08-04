$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" clone $scriptPath/appsettings.json
dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" build $scriptPath/appsettings.json
dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" run $scriptPath/appsettings.json