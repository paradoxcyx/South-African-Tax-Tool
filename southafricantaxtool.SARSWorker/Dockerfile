FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["southafricantaxtool.SARSWorker/southafricantaxtool.SARSWorker.csproj", "southafricantaxtool.SARSWorker/"]
RUN dotnet restore "southafricantaxtool.SARSWorker/southafricantaxtool.SARSWorker.csproj"
COPY . .
WORKDIR "/src/southafricantaxtool.SARSWorker"
RUN dotnet build "southafricantaxtool.SARSWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "southafricantaxtool.SARSWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "southafricantaxtool.SARSWorker.dll"]
