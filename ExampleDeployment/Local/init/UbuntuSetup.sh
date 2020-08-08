#!/bin/bash

arch="$1"
echo "Using arch '$arch' for docker."

# Setup Firewall
sudo ufw allow http
sudo ufw allow https
sudo ufw allow 22
sudo ufw --force enable

# Setup Docker
sudo apt-get update
sudo apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common -y
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo apt-key fingerprint 0EBFCD88
sudo add-apt-repository "deb [arch=$arch] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io -y

# Setup Network
sudo docker network create -d bridge appnet

# Create app dir
sudo mkdir /app