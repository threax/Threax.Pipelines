# Setup A New Server
This document describes how to setup a new Ubuntu VM to run these apps.

## Setup Firewall
```
sudo ufw enable && sudo ufw allow http && sudo ufw allow https && sudo ufw allow 22
```

## Install Docker
```
sudo apt-get update
sudo apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common -y
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -
sudo apt-key fingerprint 0EBFCD88
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable"
sudo apt-get update
sudo apt-get install docker-ce docker-ce-cli containerd.io -y
```

## Create Network
```
sudo docker network create -d bridge appnet
```

## Install Threax.DockerBuild
Copy this file over, it is self contained.

## Setup Nginx
Create appsettings.json in /app/nginx

Add ssl cert to nginx secrets, this needs to be created somewhere else like a self signed or let's encrypt
```
sudo /home/threax/Threax.DockerBuild setsecret /app/nginx/appsettings.json private-key /home/threax/privkey1.pem 
sudo /home/threax/Threax.DockerBuild setsecret /app/nginx/appsettings.json public-key /home/threax/fullchain1.pem 
```

Run with
```
sudo /home/threax/Threax.DockerBuild run /app/nginx/appsettings.json
```

## Setup Id server
Create appsettings.json in /app/id

Clone and build
```
sudo /home/threax/Threax.DockerBuild clone /app/id/appsettings.json
sudo /home/threax/Threax.DockerBuild build /app/id/appsettings.json
```

Create a cert by running
```
sudo docker run --rm -v /app/id/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools "createCert signing 100 /out/id-server-signing-cert"
```

Finally run the id server
```
sudo /home/threax/Threax.DockerBuild run /app/id/appsettings.json
```

Create an account on the id server and get the user id.

Add this guid to the id server as an admin by running:
```
sudo docker exec -it id dotnet /app/Threax.IdServer.dll tools "addadmin YOUR_GUID"
```

## Setup App Dashboard
Create appsettings.json in /app/appdashboard
```
sudo mkdir /app/appdashboard
sudo touch /app/appdashboard/appsettings.json && sudo chmod 666 /app/appdashboard/appsettings.json
```

Clone, build and run
```
sudo /home/threax/Threax.DockerBuild clone /app/appdashboard/appsettings.json
sudo /home/threax/Threax.DockerBuild build /app/appdashboard/appsettings.json
sudo /home/threax/Threax.DockerBuild run /app/appdashboard/appsettings.json
```