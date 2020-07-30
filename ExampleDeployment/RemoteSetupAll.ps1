param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
&"$scriptPath/init/Remote.ps1" $sshConnection
&"$scriptPath/nginx/Remote.ps1" $sshConnection
&"$scriptPath/id/Remote.ps1" $sshConnection
&"$scriptPath/appdashboard/Remote.ps1" $sshConnection
'*************************************************'
'Remote Server Setup Complete'
'Visit https://id.dev.threax.com/Manage/Index and create an account.'
$userId = Read-Host -Prompt 'Enter the new user''s User Id here'
&"$scriptPath/id/AddAdmin.ps1" $sshConnection $userId
'Visit https://appdashboard.dev.threax.com to test the account.'