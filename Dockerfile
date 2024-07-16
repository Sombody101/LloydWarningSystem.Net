# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ./LloydWarningSystem.Net ./
RUN dotnet restore
RUN dotnet publish -c Release -o lloyd-bot

# Image
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
RUN apk add --no-cache icu-libs
WORKDIR /app
COPY --from=build /src/lloyd-bot .
WORKDIR /config 

ENTRYPOINT ["dotnet", "/app/LloydWarningSystem.Net.dll"]