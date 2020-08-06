FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Threax.DockerTools
RUN dotnet restore Threax.DockerTools.csproj

FROM build AS publish
ARG TARGET=ubuntu.18.04-x64
RUN dotnet publish Threax.DockerTools.csproj -r $TARGET -c Release /p:PublishTrimmed=true -o /app/bin

FROM base AS final
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /app
COPY --from=publish /app .
CMD cp -r /app/* /out
