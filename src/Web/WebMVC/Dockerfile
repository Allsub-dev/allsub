#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "allsub.sln" "allsub.sln"

COPY "Common/Common/Common.csproj" "Common/Common/Common.csproj"
COPY "Common/CommonCore/CommonCore.csproj" "Common/CommonCore/CommonCore.csproj"
COPY "Common/EventBusRabbitMQ/EventBusRabbitMQ.csproj" "Common/EventBusRabbitMQ/EventBusRabbitMQ.csproj"
COPY "Common/OAuth/OAuth.csproj" "Common/OAuth/OAuth.csproj"
COPY "Web/WebMVC/WebMVC.csproj" "Web/WebMVC/WebMVC.csproj"
COPY ["Services/TestAdService/TestAdService.csproj", "Services/TestAdService/TestAdService.csproj"]
COPY ["Services/YtService/YtService.csproj", "Services/YtService/YtService.csproj"]
COPY ["Services/VkService/VkService.csproj", "Services/VkService/VkService.csproj"]

COPY "docker-compose.dcproj" "docker-compose.dcproj"

RUN dotnet restore "allsub.sln"

COPY . .
WORKDIR /src/Web/WebMVC
RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY --from=publish /src/WebMVC.pfx /app/WebMVC.pfx
COPY --from=publish /src/appsecrets.json /app/appsecrets.json
ENTRYPOINT ["dotnet", "WebMVC.dll"]
