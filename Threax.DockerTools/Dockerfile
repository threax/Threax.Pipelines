# syntax = docker/dockerfile:1.0-experimental
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Threax.DockerTools
RUN --mount=type=cache,target=/root/.nuget/packages,id=threax-build-dotnet-310-nuget dotnet restore Threax.DockerTools.csproj

FROM build AS publish
ARG TARGET=ubuntu.18.04-x64
RUN --mount=type=cache,target=/root/.nuget/packages,id=threax-build-dotnet-310-nuget dotnet publish Threax.DockerTools.csproj -r $TARGET -c Release -o /app/bin

FROM base AS final
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /app
COPY --from=publish /app .
CMD cp -r /app/* /out
