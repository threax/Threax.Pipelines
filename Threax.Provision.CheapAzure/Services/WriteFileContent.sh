#!/bin/bash

export destDir="$(dirname "${file}")"
sudo mkdir -p $destDir
sudo echo $content > $file
if [ $? -ne 0 ]
then 
	echo "Could not write file content."
	echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
	exit -1
fi