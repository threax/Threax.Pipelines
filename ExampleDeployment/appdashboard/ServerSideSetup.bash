sudo cp -r ~/appdashboard /app
rm -r ~/appdashboard

sudo Threax.DockerTools clone /app/appdashboard/appsettings.json
sudo Threax.DockerTools build /app/appdashboard/appsettings.json
sudo Threax.DockerTools run /app/appdashboard/appsettings.json