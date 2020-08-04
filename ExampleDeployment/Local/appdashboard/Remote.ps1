param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp "$scriptPath/appsettings.json" "${sshConnection}:~/$dirName"
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"

# Clone and Build
ssh -t $sshConnection "sudo Threax.DockerTools clone /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo Threax.DockerTools build /app/$dirName/appsettings.json"

# Setup Client Secret
$clientSecretName = "JwtAuth__ClientSecret";
$secretName = $clientSecretName
ssh $sshConnection "if test -f /app/$dirname/secrets/$secretName; then exit 0; else exit 1; fi"
if($LASTEXITCODE -eq 1) {
    ssh -t $sshConnection "openssl rand -base64 32 > ~/$secretName; sudo Threax.DockerTools setsecret /app/$dirName/appsettings.json $secretName ~/$secretName; rm ~/$secretName"
}

# Setup App Dashboard in Id Server
ssh -t $sshConnection "sudo mkdir -p /app/id/data/load/$dirName"
ssh -t $sshConnection "sudo cp /app/$dirname/secrets/$clientSecretName /app/id/data/load/$dirName/$clientSecretName"
ssh -t $sshConnection "sudo chown -R 19999:19999 /app/id/data/load/$dirName"
ssh -t $sshConnection "sudo Threax.DockerTools exec /app/id/appsettings.json SetupAppDashboard /load/$dirName/$clientSecretName"
ssh -t $sshConnection "sudo rm -r /app/id/data/load/$dirName"

# Run
ssh -t $sshConnection "sudo Threax.DockerTools run /app/$dirName/appsettings.json"