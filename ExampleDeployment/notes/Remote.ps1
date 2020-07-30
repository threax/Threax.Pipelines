param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp "$scriptPath/appsettings.json" "${sshConnection}:~/$dirName"
scp "$scriptPath/appsettings.secrets.json" "${sshConnection}:~/$dirName"

# Run Server Side Setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "sudo Threax.DockerTools clone /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo Threax.DockerTools build /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo Threax.DockerTools run /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo rm /app/$dirName/appsettings.secrets.json"