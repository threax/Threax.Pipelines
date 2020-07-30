#!/bin/bash

# Setup Firewall
sudo ufw allow http
sudo ufw allow https
sudo ufw allow 22
sudo ufw --force enable

# Setup Docker
sudo apt-get update && \
sudo apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common -y && \
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add - && \
sudo apt-key fingerprint 0EBFCD88 && \
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" && \
sudo apt-get update && \
sudo apt-get install docker-ce docker-ce-cli containerd.io -y

# Setup Network
sudo docker network create -d bridge appnet

# Setup Threax.DockerTools
curl -L https://github.com/threax/Threax.Pipelines/releases/download/vThreax.DockerTools_1.0.0-pre01/Threax.DockerTools > ~/Threax.DockerTools && \
sudo mv ~/Threax.DockerTools /bin/Threax.DockerTools && \
sudo chmod 700 /bin/Threax.DockerTools

# Create app dir
sudo mkdir /app