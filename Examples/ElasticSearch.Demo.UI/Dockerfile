#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["ElasticSearch.Demo.UI/ElasticSearch.Demo.UI.csproj", "ElasticSearch.Demo.UI/"]
RUN dotnet restore "ElasticSearch.Demo.UI/ElasticSearch.Demo.UI.csproj"
COPY . .
WORKDIR "/src/ElasticSearch.Demo.UI"
RUN dotnet build "ElasticSearch.Demo.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ElasticSearch.Demo.UI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ElasticSearch.Demo.UI.dll"]