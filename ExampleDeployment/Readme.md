# Setup A New Server

## Setup Firewall
```
sudo ufw enable && \
sudo ufw allow http && \
sudo ufw allow https && \
sudo ufw allow 22
```

## Install Docker
```
sudo apt-get update && \
sudo apt-get install apt-transport-https ca-certificates curl gnupg-agent software-properties-common -y && \
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add - && \
sudo apt-key fingerprint 0EBFCD88 && \
sudo add-apt-repository "deb [arch=amd64] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" && \
sudo apt-get update && \
sudo apt-get install docker-ce docker-ce-cli containerd.io -y
```

## Create Network
```
sudo docker network create -d bridge appnet
```

## Install Threax.DockerTools
```
curl -L https://github.com/threax/Threax.Pipelines/releases/download/vThreax.DockerTools_1.0.0-pre01/Threax.DockerTools > ~/Threax.DockerTools && \
sudo mv ~/Threax.DockerTools /bin/Threax.DockerTools && \
sudo chmod 777 /bin/Threax.DockerTools
```

## Setup Nginx
Create appsettings.json in /app/nginx
```
sudo mkdir /app/nginx && \
sudo touch /app/nginx/appsettings.json && \
sudo chmod 666 /app/nginx/appsettings.json && \
sudo touch /app/nginx/Dockerfile && \
sudo chmod 666 /app/nginx/Dockerfile && \
sudo touch /app/nginx/nginx.conf && \
sudo chmod 666 /app/nginx/nginx.conf
```
Copy the contents from the nginx example directory.

Add ssl cert to nginx secrets, this needs to be created somewhere else like a self signed or let's encrypt. Copy the certs to `/app/cert/`.
```
sudo Threax.DockerTools setsecret /app/nginx/appsettings.json private-key /app/cert/privkey1.pem && \
sudo Threax.DockerTools setsecret /app/nginx/appsettings.json public-key /app/cert/fullchain1.pem
```

Remove the cert:
```
sudo rm -r /app/cert
```

Build and Run with
```
sudo Threax.DockerTools build /app/nginx/appsettings.json && \
sudo Threax.DockerTools run /app/nginx/appsettings.json
```

## Setup Id server
Create appsettings.json in /app/id
```
sudo mkdir /app/id && \
sudo touch /app/id/appsettings.json && \
sudo chmod 666 /app/id/appsettings.json
```
Copy contents from example folder.

Clone and build
```
sudo Threax.DockerTools clone /app/id/appsettings.json && \
sudo Threax.DockerTools build /app/id/appsettings.json
```

Create a cert by running
```
sudo docker run --rm -v /app/id/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools "createCert signing 100 /out/id-server-signing-cert"
```

Finally run the id server
```
sudo Threax.DockerTools run /app/id/appsettings.json
```

Create an account on the id server and get the user id.

Add this guid to the id server as an admin by running:
```
sudo docker exec -it id dotnet /app/Threax.IdServer.dll tools "addadmin YOUR_GUID"
```

## Setup App Dashboard
Create appsettings.json in /app/appdashboard
```
sudo mkdir /app/appdashboard && \
sudo touch /app/appdashboard/appsettings.json && \
sudo chmod 666 /app/appdashboard/appsettings.json
```
Copy contents from example folder.

Clone, build and run
```
sudo Threax.DockerTools clone /app/appdashboard/appsettings.json && \
sudo Threax.DockerTools build /app/appdashboard/appsettings.json && \
sudo Threax.DockerTools run /app/appdashboard/appsettings.json
```