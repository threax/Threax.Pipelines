param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy files
scp "$scriptPath/appsettings.json" "${sshConnection}:~/$dirName"
scp "$scriptPath/BuildAndRun.sh" "${sshConnection}:~/$dirName"

# Run Server Side Setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "sudo bash /app/$dirName/BuildAndRun.sh"