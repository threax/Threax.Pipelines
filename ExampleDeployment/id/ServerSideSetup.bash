sudo cp -r ~/id /app
rm -r ~/id

sudo Threax.DockerTools clone /app/id/appsettings.json
sudo Threax.DockerTools build /app/id/appsettings.json

sudo docker run --rm -v /app/id/secrets:/out threaxacr.azurecr.io/id:threaxpipe-current tools "createCert signing 100 /out/id-server-signing-cert"

sudo Threax.DockerTools run /app/id/appsettings.json