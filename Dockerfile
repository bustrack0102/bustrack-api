# Imagen de producción BusTrack API (multi-stage, sin secretos en capas intermedias).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY nuget.config ./
COPY Directory.Build.props ./
COPY API_SDD.slnx ./
COPY API_SDD/API_SDD.csproj API_SDD/
COPY UnitTest/UnitTest.csproj UnitTest/
COPY IntegrationTest/IntegrationTest.csproj IntegrationTest/
COPY ContractTest/ContractTest.csproj ContractTest/
COPY SecurityTest/SecurityTest.csproj SecurityTest/
COPY DatabaseTest/DatabaseTest.csproj DatabaseTest/

RUN dotnet restore API_SDD.slnx

COPY API_SDD/ API_SDD/

RUN dotnet publish API_SDD/API_SDD.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN mkdir -p /app/keys
COPY --from=build /app/publish ./
ENV PORT=8000 ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8000
ENTRYPOINT ["dotnet", "APISDD.dll"]
