FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["RouteEvidence.csproj", "./"]
RUN dotnet restore "RouteEvidence.csproj"

COPY . .
RUN dotnet publish RouteEvidence.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Cloud Run requiere el puerto 8080 por defecto
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RouteEvidence.dll"]
