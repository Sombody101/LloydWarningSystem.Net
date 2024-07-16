FROM mcr.microsoft.com/dotnet/core/sdk:8.0 

WORKDIR /app

COPY . .
RUN dotnet restore

# Adjust port if needed (console apps typically don't expose ports)
# EXPOSE 8080  

CMD ["dotnet", "run", "LloydWarningSystem.Net.dll"] 