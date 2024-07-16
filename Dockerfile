# BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ./LloydWarningSystem.Net ./app
RUN dotnet publish -c Release -o out

# RUNNER IMAGE
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine
RUN apk add --no-cache icu-libs
WORKDIR /app
COPY --from=build /src/out .
WORKDIR /config 

ENTRYPOINT ["dotnet", "/app/LloydWarningSystem.Net.dll"]