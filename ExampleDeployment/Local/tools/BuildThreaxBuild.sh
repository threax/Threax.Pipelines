#!/bin/bash

# Thanks to https://stackoverflow.com/questions/59895/how-to-get-the-source-directory-of-a-bash-script-from-within-the-script-itself?page=1&tab=votes#tab-top
SOURCE="${BASH_SOURCE[0]}"
while [ -h "$SOURCE" ]; do # resolve $SOURCE until the file is no longer a symlink
  DIR="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"
  SOURCE="$(readlink "$SOURCE")"
  [[ $SOURCE != /* ]] && SOURCE="$DIR/$SOURCE" # if $SOURCE was a relative symlink, we need to resolve it relative to the path where the symlink file was located
done
scriptPath="$( cd -P "$( dirname "$SOURCE" )" >/dev/null 2>&1 && pwd )"

toolsRepo="https://github.com/threax/build-threax.git"
srcDir="$scriptPath/src/build-threax"
if [ -d $srcDir ]
then
    pushd $srcDir
    git pull
    popd
else
    git clone $toolsRepo $srcDir
fi

bash $srcDir/Build.sh