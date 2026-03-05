# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/WorkflowApi.API/WorkflowApi.API.csproj", "WorkflowApi.API/"]
COPY ["src/WorkflowApi.Infrastructure/WorkflowApi.Infrastructure.csproj", "WorkflowApi.Infrastructure/"]
COPY ["src/WorkflowApi.Application/WorkflowApi.Application.csproj", "WorkflowApi.Application/"]
COPY ["src/WorkflowApi.Domain/WorkflowApi.Domain.csproj", "WorkflowApi.Domain/"]

RUN dotnet restore "WorkflowApi.API/WorkflowApi.API.csproj"

COPY src/ WorkflowApi.API/../

WORKDIR "/src/WorkflowApi.API"
RUN dotnet build "WorkflowApi.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "WorkflowApi.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WorkflowApi.API.dll"]
