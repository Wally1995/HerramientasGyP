FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY HerramientasGyP.Api/HerramientasGyP.Api.csproj HerramientasGyP.Api/
COPY HerramientasGyP.Api.DataAccess/HerramientasGyP.Api.DataAccess.csproj HerramientasGyP.Api.DataAccess/
COPY HerramientasGyP.Api.Models/HerramientasGyP.Api.Models.csproj HerramientasGyP.Api.Models/

RUN dotnet restore "HerramientasGyP.Api/HerramientasGyP.Api.csproj"

COPY . .

WORKDIR /src/HerramientasGyP.Api

RUN dotnet build "HerramientasGyP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HerramientasGyP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HerramientasGyP.Api.dll"]
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY HerramientasGyP.Api/HerramientasGyP.Api.csproj HerramientasGyP.Api/
COPY HerramientasGyP.Api.DataAccess/HerramientasGyP.Api.DataAccess.csproj HerramientasGyP.Api.DataAccess/
COPY HerramientasGyP.Api.Models/HerramientasGyP.Api.Models.csproj HerramientasGyP.Api.Models/

RUN dotnet restore "HerramientasGyP.Api/HerramientasGyP.Api.csproj"

COPY . .

WORKDIR /src/HerramientasGyP.Api

RUN dotnet build "HerramientasGyP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HerramientasGyP.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "HerramientasGyP.Api.dll"]
