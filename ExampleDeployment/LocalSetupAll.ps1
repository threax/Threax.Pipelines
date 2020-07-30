$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

&"$scriptPath/init/Local.ps1"
&"$scriptPath/nginx/Local.ps1"
&"$scriptPath/id/Local.ps1"
&"$scriptPath/appdashboard/Local.ps1"
'*************************************************'
'Local Server Setup Complete'
'Visit https://id.dev.threax.com/Manage/Index and create an account.'
$userId = Read-Host -Prompt 'Enter the new user''s User Id here'
Threax.DockerTools exec $scriptPath/appsettings.json AddAdmin $userId
'Visit https://appdashboard.dev.threax.com to test the account.'