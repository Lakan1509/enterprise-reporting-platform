FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/EnterpriseReporting.Web/EnterpriseReporting.Web.csproj", "src/EnterpriseReporting.Web/"]

RUN dotnet restore "src/EnterpriseReporting.Web/EnterpriseReporting.Web.csproj"

COPY . .

WORKDIR "/src/src/EnterpriseReporting.Web"

RUN dotnet publish \
    "EnterpriseReporting.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EnterpriseReporting.Web.dll"]
