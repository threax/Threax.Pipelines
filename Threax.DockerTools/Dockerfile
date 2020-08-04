FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM threax/build-dotnet:3.1.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Threax.DockerTools
RUN dotnet restore Threax.DockerTools.csproj
RUN dotnet build Threax.DockerTools.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Threax.DockerTools.csproj -c Release -o /app

FROM base AS final
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# install docker
RUN curl -sSL https://get.docker.com/ | sh

WORKDIR /app
COPY --from=publish /app .
