param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/Build.sh" "${sshConnection}:~/$dirName"
scp -r "$scriptPath/BuildDockerTools.sh" "${sshConnection}:~/$dirName"
scp -r "$scriptPath/BuildThreaxBuild.sh" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "bash /app/$dirName/Build.sh"