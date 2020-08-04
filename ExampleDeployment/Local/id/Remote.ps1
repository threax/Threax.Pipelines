param ([Parameter(Position=0,mandatory=$true)]$sshConnection)

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

#Copy Files
scp "$scriptPath/appsettings.json" "${sshConnection}:~/$dirName"

# Run Server Side Setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "sudo Threax.DockerTools clone /app/$dirName/appsettings.json"
ssh -t $sshConnection "sudo Threax.DockerTools build /app/$dirName/appsettings.json"
ssh $sshConnection "if test -f /app/$dirname/secrets/id-server-signing-cert; then exit 0; else exit 1; fi"
if($LASTEXITCODE -eq 1) {
    Write-Host "Creating Id Server Signing Certificate"
    ssh -t $sshConnection "sudo docker run --rm -v /app/$dirName/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools 'createCert signing 100 /out/id-server-signing-cert'"
}

ssh -t $sshConnection "sudo Threax.DockerTools run /app/$dirName/appsettings.json"