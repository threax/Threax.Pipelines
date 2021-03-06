param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/UbuntuSetup.sh" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "chmod 777 ~/$dirName/UbuntuSetup.sh;~/$dirName/UbuntuSetup.sh;rm -r ~/init"