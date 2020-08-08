docker rm $(docker ps -a -q) --force
docker system prune -a --force