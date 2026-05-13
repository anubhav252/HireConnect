FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HireConnect.Interview/HireConnect.Interview.csproj", "HireConnect.Interview/"]
RUN dotnet restore "HireConnect.Interview/HireConnect.Interview.csproj"
COPY HireConnect.Interview/ HireConnect.Interview/
WORKDIR "/src/HireConnect.Interview"
RUN dotnet build "HireConnect.Interview.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HireConnect.Interview.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HireConnect.Interview.dll"]
