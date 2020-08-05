$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/BuildThreaxBuild.ps1"
&"$scriptPath/BuildDockerTools.ps1"