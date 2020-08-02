#!/bin/bash

destDir="$(dirname "${file}")"
sudo -u $user mkdir -p $destDir
sudo -u $user echo $content > $file
if [ $? -ne 0 ]
then 
	echo "Could not write file content."
	echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
	exit 1
fi

sudo -u $user Threax.DockerTools run $file
if [ $? -ne 0 ]
then 
	echo "Problem during Threax.DockerTools run."
	echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
	exit 1
fi