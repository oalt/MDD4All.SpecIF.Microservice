#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

WORKDIR /app

ENV ASPNETCORE_URLS https://localhost:888
ENV ASPNETCORE_URLS http://localhost:887


FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["MDD4All.SpecIF.Microservice/MDD4All.SpecIf.Microservice-dev.csproj", "MDD4All.SpecIF.Microservice/"]
COPY ["MDD4All.SpecIF.DataProvider.Integration/src/MDD4All.SpecIF.DataProvvider.Integration/MDD4All.SpecIF.DataProvider.Integration-dev.csproj", "MDD4All.SpecIF.DataProvider.Integration/src/MDD4All.SpecIF.DataProvvider.Integration/"]
COPY ["MDD4All.SpecIF.ServiceDataProvider/src/MDD4All.SpecIF.ServiceDataProvvider/MDD4All.SpecIF.ServiceDataProvider-dev.csproj", "MDD4All.SpecIF.ServiceDataProvider/src/MDD4All.SpecIF.ServiceDataProvvider/"]
COPY ["MDD4All.SpecIF.DataModels/src/MDD4All.SpecIF.DataModels/MDD4All.SpecIF.DataModels.csproj", "MDD4All.SpecIF.DataModels/src/MDD4All.SpecIF.DataModels/"]
COPY ["MDD4All.SpecIF.DataProvider.WebAPI/src/MDD4All.SpecIF.DataProvvider.WebAPI/MDD4All.SpecIF.DataProvider.WebAPI-dev.csproj", "MDD4All.SpecIF.DataProvider.WebAPI/src/MDD4All.SpecIF.DataProvvider.WebAPI/"]
COPY ["MDD4All.SpecIF.DataModels.RightsManagement/src/MDD4All.SpecIF.DataModels.RightsManagement/MDD4All.SpecIF.DataModels.RightsManagement.csproj", "MDD4All.SpecIF.DataModels.RightsManagement/src/MDD4All.SpecIF.DataModels.RightsManagement/"]
COPY ["MDD4All.SpecIF.DataProvider.Contracts/src/MDD4All.SpecIF.DataProvider.Contracts/MDD4All.SpecIF.DataProvider.Contracts-dev.csproj", "MDD4All.SpecIF.DataProvider.Contracts/src/MDD4All.SpecIF.DataProvider.Contracts/"]
COPY ["MDD4All.SpecIF.ViewModels.IntegrationService/src/MDD4All.SpecIF.ViewModels.IntegrationService/MDD4All.SpecIF.ViewModels.IntegrationService-dev.csproj", "MDD4All.SpecIF.ViewModels.IntegrationService/src/MDD4All.SpecIF.ViewModels.IntegrationService/"]
COPY ["MDD4All.SpecIF.DataProvider.MongoDB/src/MDD4All.SpecIF.DataProvider.MongoDB/MDD4All.SpecIF.DataProvider.MongoDB-dev.csproj", "MDD4All.SpecIF.DataProvider.MongoDB/src/MDD4All.SpecIF.DataProvider.MongoDB/"]
COPY ["MDD4All.SpecIF.DataProvider.EA/src/MDD4All.SpecIF.DataProvider.EA/MDD4All.SpecIF.DataProvider.EA-dev.csproj", "MDD4All.SpecIF.DataProvider.EA/src/MDD4All.SpecIF.DataProvider.EA/"]
COPY ["MDD4All.SpecIF.DataIntegrator.KafkaListener/src/MDD4All.SpecIF.DataIntegrator.KafkaListener/MDD4All.SpecIF.DataIntegrator.KafkaListener-dev.csproj", "MDD4All.SpecIF.DataIntegrator.KafkaListener/src/MDD4All.SpecIF.DataIntegrator.KafkaListener/"]
COPY ["MDD4All.SpecIF.DataIntegrator.Contracts/src/MDD4All.SpecIF.DataIntegrator.Contracts/MDD4All.SpecIF.DataIntegrator.Contracts-dev.csproj", "MDD4All.SpecIF.DataIntegrator.Contracts/src/MDD4All.SpecIF.DataIntegrator.Contracts/"]
COPY ["MDD4All.Kafka.DataAccess/src/MDD4All.Kafka.DataAccess/MDD4All.Kafka.DataAccess-dev.csproj", "MDD4All.Kafka.DataAccess/src/MDD4All.Kafka.DataAccess/"]
COPY ["MDD4All.SpecIF.DataProvider.Jira/src/MDD4All.SpecIF.DataProvider.Jira/MDD4All.SpecIF.DataProvider.Jira-dev.csproj", "MDD4All.SpecIF.DataProvider.Jira/src/MDD4All.SpecIF.DataProvider.Jira/"]
COPY ["MDD4All.Jira.DataAccess/src/MDD4All.Jira.DataAccess/MDD4All.Jira.DataAccess-dev.csproj", "MDD4All.Jira.DataAccess/src/MDD4All.Jira.DataAccess/"]
COPY ["MDD4All.Jira.DataModels/src/MDD4All.Jira.DataModels/MDD4All.Jira.DataModels.csproj", "MDD4All.Jira.DataModels/src/MDD4All.Jira.DataModels/"]
COPY ["MDD4All.SpecIF.DataAccess.Jira/src/MDD4All.SpecIF.DataAccess.Jira/MDD4All.SpecIF.DataAccess.Jira-dev.csproj", "MDD4All.SpecIF.DataAccess.Jira/src/MDD4All.SpecIF.DataAccess.Jira/"]
COPY ["MDD4All.SpecIF.DataFactory/src/MDD4All.SpecIF.DataFactory/MDD4All.SpecIF.DataFactory-dev.csproj", "MDD4All.SpecIF.DataFactory/src/MDD4All.SpecIF.DataFactory/"]
COPY ["MDD4All.SpecIF.DataModels.Manipulation/src/MDD4All.SpecIF.DataModels.Manipulation/MDD4All.SpecIF.DataModels.Manipulation-dev.csproj", "MDD4All.SpecIF.DataModels.Manipulation/src/MDD4All.SpecIF.DataModels.Manipulation/"]
COPY ["MDD4All.SpecIF.ViewModels.SpecIfEvent/src/MDD4All.SpecIF.ViewModels.SpecIfEvent/MDD4All.SpecIF.ViewModels.SpecIfEvent-dev.csproj", "MDD4All.SpecIF.ViewModels.SpecIfEvent/src/MDD4All.SpecIF.ViewModels.SpecIfEvent/"]
COPY ["MDD4All.SpecIF.DataIntegrator.EA/src/MDD4All.SpecIF.DataIntegrator.EA/MDD4All.SpecIF.DataIntegrator.EA-dev.csproj", "MDD4All.SpecIF.DataIntegrator.EA/src/MDD4All.SpecIF.DataIntegrator.EA/"]
COPY ["MDD4All.SpecIF.DataProvider.File/src/MDD4All.SpecIF.DataProvider.File/MDD4All.SpecIF.DataProvider.File-dev.csproj", "MDD4All.SpecIF.DataProvider.File/src/MDD4All.SpecIF.DataProvider.File/"]

RUN dotnet restore "MDD4All.SpecIF.Microservice/MDD4All.SpecIf.Microservice-dev.csproj"
COPY . .
WORKDIR "/src/MDD4All.SpecIF.Microservice"
RUN dotnet build "MDD4All.SpecIf.Microservice-dev.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MDD4All.SpecIf.Microservice-dev.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MDD4All.SpecIf.Microservice.dll"]