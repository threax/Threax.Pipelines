#!/bin/bash

# Thanks to https://stackoverflow.com/questions/59895/how-to-get-the-source-directory-of-a-bash-script-from-within-the-script-itself?page=1&tab=votes#tab-top
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
scriptPath="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"

$scriptPath/../tools/bin/Threax.DockerTools setsecret $scriptPath/appsettings.json private-key $scriptPath/../cert/privkey1.pem
$scriptPath/../tools/bin/Threax.DockerTools setsecret $scriptPath/appsettings.json public-key $scriptPath/../cert/fullchain1.pem

$scriptPath/../tools/bin/Threax.DockerTools build $scriptPath/appsettings.json
$scriptPath/../tools/bin/Threax.DockerTools run $scriptPath/appsettings.json