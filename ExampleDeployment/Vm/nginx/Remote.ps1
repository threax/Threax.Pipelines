param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy files
scp -r "$scriptPath/../cert" ${sshConnection}:~/
scp "$scriptPath/appsettings.json" "${sshConnection}:~/$dirName"
scp "$scriptPath/Dockerfile" "${sshConnection}:~/$dirName"
scp "$scriptPath/nginx.conf" "${sshConnection}:~/$dirName"

# Run Server Side Setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "sudo cp -r ~/cert /app;rm -r ~/cert"

ssh -t $sshConnection "sudo Threax.DockerTools setsecret /app/$dirName/appsettings.json private-key /app/cert/privkey1.pem"
ssh -t $sshConnection "sudo Threax.DockerTools setsecret /app/$dirName/appsettings.json public-key /app/cert/fullchain1.pem"

ssh -t $sshConnection "sudo Threax.DockerTools build /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo Threax.DockerTools run /app/$dirName/appsettings.json"

ssh -t $sshConnection "sudo rm -r /app/cert"