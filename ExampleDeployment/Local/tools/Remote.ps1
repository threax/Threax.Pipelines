param (
    [Parameter(Position=0,mandatory=$true)]$sshConnection, 
    [Parameter(Position=1,mandatory=$true)]$arch # Supported: x86_64
)

if($arch -eq "x86_64") {
    $buildTarget = "linux-x64"
}
elseif($arch -eq "aarch64") {
    $buildTarget = "linux-arm64"
}
else {
    $buildTarget = Read-Host -Prompt "Unknown server architecture '$targetArch'. Please enter the tools build target (linux-x64, linux-arm64 etc):"
}

Write-Host "Tools Build target is '$buildTarget'."

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/Build.sh" "${sshConnection}:~/$dirName"
scp -r "$scriptPath/BuildDockerTools.sh" "${sshConnection}:~/$dirName"
scp -r "$scriptPath/BuildThreaxBuild.sh" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "sudo cp -r ~/$dirName /app;rm -r ~/$dirName"
ssh -t $sshConnection "bash /app/$dirName/Build.sh $buildTarget"