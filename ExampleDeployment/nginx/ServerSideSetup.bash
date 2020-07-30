sudo cp -r ~/nginx /app
rm -r ~/nginx

sudo Threax.DockerTools setsecret /app/nginx/appsettings.json private-key ~/cert/privkey1.pem && \
sudo Threax.DockerTools setsecret /app/nginx/appsettings.json public-key ~/cert/fullchain1.pem
rm -r ~/cert

sudo Threax.DockerTools build /app/nginx/appsettings.json && \
sudo Threax.DockerTools run /app/nginx/appsettings.json