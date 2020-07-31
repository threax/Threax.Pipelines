#!/bin/bash

export destDir="$(dirname "${file}")"
sudo mkdir -p $destDir
sudo echo $content > $file