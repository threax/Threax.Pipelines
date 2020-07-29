# Threax.DockerTools
This application simplifies running apps on a single node server using straight Docker.

It works by placing appsettings.json files on the server to create a root app directory. Typically these go into
/app/name where name is the name of this specific app. From these source folders the following directories will be created by the tool:
 * src - Source code clone folder if repo is specified.
 * data - Data folder for the app.
 * secrets - Folder to store secrets for the app.

These folders will be scoped to only be accessible by the user id specified for the app.

## Building
Run Publish.ps1 to create a build for Ubuntu.

## Clone
Clone a repo by running:
```
Threax.DockerTools clone appsettings.json
```
This will clone the repository specified in the appsettings.json file.

## Build
Build a dockerfile by running:
```
Threax.DockerTools build appsettings.json
```
This will build the Dockerfile specified in the appsettings.json file.

## Run
Run an app by running:
```
Threax.DockerTools run appsettings.json
```
This will start the app specified in the appsettings.json file.

## Set Secret
Set a secret for an app with:
```
Threax.DockerTools setsecret appsettings.json secret-name /path/to/source
```
This adds a secret to an app's secrets folder.