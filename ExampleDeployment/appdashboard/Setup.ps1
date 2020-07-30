param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
scp -r "$scriptPath/../$dirName" ${sshConnection}:~/
ssh -t $sshConnection "chmod 777 ~/$dirName/ServerSideSetup.bash;~/$dirName/ServerSideSetup.bash;"