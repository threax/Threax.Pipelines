#!/bin/bash

sudo Threax.DockerTools "exec" "$file" $arg0 $arg1 $arg2 $arg3 $arg4
if [ $? -ne 0 ]
then 
	echo "Problem during Threax.DockerTools run."
	echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
	exit 1
fi