param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Connect ssh key
Get-Content ~\.ssh\id_rsa.pub | ssh $sshConnection "umask 077; test -d .ssh || mkdir .ssh ; cat > .ssh/authorized_keys || exit 1"

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/UbuntuSetup.bash" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "chmod 777 ~/$dirName/UbuntuSetup.bash;~/$dirName/UbuntuSetup.bash;rm -r ~/init"