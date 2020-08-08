param (
    [Parameter(Position=0,mandatory=$true)]$sshConnection, 
    [Parameter(Position=1,mandatory=$true)]$arch # Supported: x86_64
)

if($arch -eq "x86_64") {
    $dockerArch = "amd64"
}
elseif($arch -eq "aarch64") {
    $dockerArch = "arm64"
}
else {
    $dockerArch = Read-Host -Prompt "Unknown server architecture '$targetArch'. Please enter the docker architecture (amd64, arm64 etc):"
}

Write-Host "Docker arch is '$dockerArch'."

# Initialize
$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$dirName = [System.IO.Path]::GetFileName($scriptPath)
ssh -t $sshConnection "mkdir ~/$dirName"

# Copy Files
scp -r "$scriptPath/UbuntuSetup.sh" "${sshConnection}:~/$dirName"

# Run server side setup
ssh -t $sshConnection "bash ~/$dirName/UbuntuSetup.sh $dockerArch;rm -r ~/$dirName"