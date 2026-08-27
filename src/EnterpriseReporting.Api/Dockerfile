FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EnterpriseReporting.Api/EnterpriseReporting.Api.csproj", "src/EnterpriseReporting.Api/"]
COPY ["src/EnterpriseReporting.Domain/EnterpriseReporting.Domain.csproj", "src/EnterpriseReporting.Domain/"]
COPY ["src/EnterpriseReporting.Infrastructure/EnterpriseReporting.Infrastructure.csproj", "src/EnterpriseReporting.Infrastructure/"]

RUN dotnet restore "src/EnterpriseReporting.Api/EnterpriseReporting.Api.csproj"

COPY . .

WORKDIR "/src/src/EnterpriseReporting.Api"

RUN dotnet publish \
    "EnterpriseReporting.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EnterpriseReporting.Api.dll"]
