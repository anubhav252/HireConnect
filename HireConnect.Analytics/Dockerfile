FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HireConnect.Analytics/HireConnect.Analytics.csproj", "HireConnect.Analytics/"]
RUN dotnet restore "HireConnect.Analytics/HireConnect.Analytics.csproj"
COPY HireConnect.Analytics/ HireConnect.Analytics/
WORKDIR "/src/HireConnect.Analytics"
RUN dotnet build "HireConnect.Analytics.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HireConnect.Analytics.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HireConnect.Analytics.dll"]
