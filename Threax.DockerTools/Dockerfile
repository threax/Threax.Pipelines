FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app

FROM threax/build-dotnet:3.1.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Threax.DockerTools
RUN dotnet restore Threax.DockerTools.csproj

FROM build AS publish
RUN dotnet publish Threax.DockerTools.csproj -r ubuntu.18.04-x64 -c Release /p:PublishTrimmed=true -o /app/ubuntu
RUN dotnet publish Threax.DockerTools.csproj -r win-x64 -c Release -o /app/win

FROM base AS final
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
WORKDIR /app
COPY --from=publish /app .
CMD cp -r /app/* /out
