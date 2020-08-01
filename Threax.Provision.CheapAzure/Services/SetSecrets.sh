#!/bin/bash

function createSecret {
	local file="$1"
	local name="$2"
	local content="$3"

	local destDir="$(dirname "${file}")"
	sudo mkdir -p "$destDir"
	
	sudo echo "$content" > "$file"
	if [ $? -ne 0 ]
	then 
		echo "Could not write file content."
		echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
		exit 1
	fi

	sudo Threax.DockerTools setsecret "$settingsFile" "$name" "$file"
	if [ $? -ne 0 ]
	then 
		echo "Problem during Threax.DockerTools setsecret."
		echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
		exit 1
	fi

	sudo rm "$file"
	if [ $? -ne 0 ]
	then 
		echo "Could not remove source file."
		echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
		exit 1
	fi
}

destDir="$(dirname "${settingsFile}")"
if [ -f $settingsFile ]
then
	echo "Settings file '$settingsFile' already exists. Leaving existing file alone."
else
	echo "No existing settings file found. Writing current settings to '$settingsFile'."
	
	sudo mkdir -p "$destDir"
	sudo echo "$settingsContent" > "$settingsFile"
	if [ $? -ne 0 ]
	then 
		echo "Could not write settings file content."
		echo "Invoke-AzVMRunCommand_ALERT_ERROR_OCCURED"
		exit 1
	fi
fi

if [ ! -z ${file0+x} ]
then
	echo "Creating secret $name0"
	createSecret "$file0" "$name0" "$content0"
fi

if [ ! -z ${file1+x} ]
then
	echo "Creating secret $name1"
	createSecret "$file1" "$name1" "$content1"
fi

if [ ! -z ${file2+x} ]
then
	echo "Creating secret $name2"
	createSecret "$file2" "$name2" "$content2"
fi

if [ ! -z ${file3+x} ]
then
	echo "Creating secret $name3"
	createSecret "$file3" "$name3" "$content3"
fi

if [ ! -z ${file4+x} ]
then
	echo "Creating secret $name4"
	createSecret "$file4" "$name4" "$content4"
fi