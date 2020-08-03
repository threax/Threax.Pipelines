param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Connect ssh key
Get-Content ~\.ssh\id_rsa.pub | ssh $sshConnection "umask 077; test -d .ssh || mkdir .ssh ; cat > .ssh/authorized_keys || exit 1"

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
&"$scriptPath/init/Remote.ps1" $sshConnection
&"$scriptPath/nginx/Remote.ps1" $sshConnection
&"$scriptPath/id/Remote.ps1" $sshConnection
&"$scriptPath/appdashboard/Remote.ps1" $sshConnection
&"$scriptPath/notes/Remote.ps1" $sshConnection

Write-Host '*************************************************'
Write-Host 'Remote Server Setup Complete'
Write-Host '*************************************************'
Write-Host 'Visit https://id.dev.threax.com/Manage/Index and create an account.'
$userId = Read-Host -Prompt 'Enter the new user''s User Id here'
ssh -t $sshConnection "sudo Threax.DockerTools exec /app/id/appsettings.json AddAdmin $userId"
ssh -t $sshConnection "sudo Threax.DockerTools exec /app/notes/appsettings.json AddAdmin $userId"
Write-Host 'Visit https://appdashboard.dev.threax.com to test the account. Also add https://notes.dev.threax.com to add the notes app to the id server.'
Write-Host 'Visit https://notes.dev.threax.com to try the notes app.'

# Disconnect ssh key
ssh $sshConnection "umask 077; rm .ssh/authorized_keys || exit 1"