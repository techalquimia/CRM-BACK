FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RouteEvidence/RouteEvidence.csproj", "RouteEvidence/"]
RUN dotnet restore "RouteEvidence/RouteEvidence.csproj"

COPY RouteEvidence/ RouteEvidence/
WORKDIR /src/RouteEvidence
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RouteEvidence.dll"]
