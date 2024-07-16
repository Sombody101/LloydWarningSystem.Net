FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build-base
WORKDIR /app

COPY LloydWarningSystem.Net.sln .
COPY LloydWarningSystem.Net ./LloydWarningSystem.Net/
COPY . .

RUN dotnet restore LloydWarningSystem.Net.sln

FROM dotnet-build-base AS dotnet-build
RUN dotnet build -maxcpucount:1 -c Release --no-restore LloydWarningSystem.Net.sln

FROM dotnet-build AS publish
RUN dotnet publish -maxcpucount:1 -c Release --no-build --no-restore -o /app  LloydWarningSystem.Net/LloydWarningSystem.Net.csproj

FROM mcr.microsoft.com/dotnet/runtime:8.0-runtime AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LloydWarningSystem.Net.dll"] 