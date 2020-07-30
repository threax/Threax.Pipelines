param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
&"$scriptPath/init/Setup.ps1" $sshConnection
&"$scriptPath/nginx/Setup.ps1" $sshConnection
&"$scriptPath/id/Setup.ps1" $sshConnection
&"$scriptPath/appdashboard/Setup.ps1" $sshConnection
'*************************************************'
'Server Setup Complete'
'Visit https://id.dev.threax.com/Manage/Index and create an account.'
$userId = Read-Host -Prompt 'Enter the new user''s User Id here'
&"$scriptPath/id/AddAdmin.ps1" $sshConnection $userId
'Visit https://appdashboard.dev.threax.com to test the account.'