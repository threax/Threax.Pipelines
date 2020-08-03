param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/UbuntuSetup.bash" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "chmod 777 ~/$dirName/UbuntuSetup.bash;~/$dirName/UbuntuSetup.bash;rm -r ~/init"