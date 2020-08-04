param (
    [Parameter(mandatory=$true)]$RootPath,
    [Parameter(Position=1, ValueFromRemainingArguments)]$Remaining
)

$scriptPath = Split-Path $script:MyInvocation.MyCommand.Path

docker run -it --rm -v /var/run/docker.sock:/var/run/docker.sock -v "${RootPath}:/apps" threax/docker-tools @Remaining