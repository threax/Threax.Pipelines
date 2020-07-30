param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

Get-Content ~\.ssh\id_rsa.pub | ssh $sshConnection "umask 077; test -d .ssh || mkdir .ssh ; cat > .ssh/authorized_keys || exit 1"

$dirName = [System.IO.Path]::GetFileName($scriptPath)
scp -r "$scriptPath/../$dirName" ${sshConnection}:~/
ssh -t $sshConnection "chmod 777 ~/$dirName/ServerSideSetup.bash;~/$dirName/ServerSideSetup.bash;"