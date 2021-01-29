# syntax = docker/dockerfile:1.0-experimental
FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /src
COPY . .
WORKDIR /src/Threax.DockerTools
RUN --mount=type=cache,target=/root/.nuget/packages,id=threax-build-dotnet-nuget dotnet restore Threax.DockerTools.csproj

FROM build AS publish
ARG TARGET=linux-x64
RUN --mount=type=cache,target=/root/.nuget/packages,id=threax-build-dotnet-nuget dotnet publish Threax.DockerTools.csproj -r $TARGET -c Release -o /app/bin

FROM base AS final
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /app
COPY --from=publish /app/bin .
CMD cp -r /app/* /out
