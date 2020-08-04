$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" clone $scriptPath/appsettings.json
dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" build $scriptPath/appsettings.json

dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" createbase64secret "$scriptPath/appsettings.json" "SharedClientCredentials__ClientSecret" 32
dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" createbase64secret "$scriptPath/appsettings.json" "JwtAuth__ClientSecret" 32

dotnet "$scriptPath/../tools/bin/Threax.DockerTools.dll" run $scriptPath/appsettings.json