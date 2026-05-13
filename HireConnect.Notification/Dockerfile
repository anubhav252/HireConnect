FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["HireConnect.Notification/HireConnect.Notification.csproj", "HireConnect.Notification/"]
RUN dotnet restore "HireConnect.Notification/HireConnect.Notification.csproj"
COPY HireConnect.Notification/ HireConnect.Notification/
WORKDIR "/src/HireConnect.Notification"
RUN dotnet build "HireConnect.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HireConnect.Notification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HireConnect.Notification.dll"]
