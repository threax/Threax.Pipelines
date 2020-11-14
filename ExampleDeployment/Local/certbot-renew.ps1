param ([Parameter(Position=0,mandatory=$true)]$commonName, [Parameter(Position=0,mandatory=$true)]$email)

$cleanCn = $commonName;
if($cleanCn.StartsWith("*.")){
    $cleanCn = $cleanCn.Substring(2);
}

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path
$ErrorActionPreference = 'Stop'

$path = "$scriptPath/certbot"

# Create temp dir
$time = (Get-Date).tostring("yyyyMMddhhmmss");
$certDirName = "cert_$time"
New-Item -itemType Directory -Path "$path" -Name $certDirName

# This is untested, but the directory will be created correctly.
$certPath = "$path/$certDirName"
Write-Host $certPath
docker run -it --rm -v ${certPath}:/etc/letsencrypt --name certbot certbot/certbot certonly --manual --server https://acme-v02.api.letsencrypt.org/directory --preferred-challenges dns --agree-tos --manual-public-ip-logging-ok --no-eff-email --email $email -d $commonName

# Create p12 format cert. No password, will use ansible vault to secure
docker run -it --rm -v ${certPath}/archive/${cleanCn}:/cert threax/openssl openssl pkcs12 -export -inkey privkey1.pem -in fullchain1.pem -out cert.p12 -passout pass:

# Copy files
$currentPath = "$scriptPath/cert"
if(Test-Path -Path $currentPath)
{
    Remove-Item -Recurse -Force $currentPath
}
New-Item -itemType Directory -Path "$scriptPath" -Name cert
Copy-Item "$certPath\archive\$cleanCn\privkey1.pem" "$currentPath"
Copy-Item "$certPath\archive\$cleanCn\fullchain1.pem" "$currentPath"
Copy-Item "$certPath\archive\$cleanCn\cert.p12" "$currentPath"