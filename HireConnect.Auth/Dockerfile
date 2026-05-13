FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["HireConnect.Auth/HireConnect.Auth.csproj", "HireConnect.Auth/"]
COPY ["HireConnect.Shared/HireConnect.Shared.csproj", "HireConnect.Shared/"]

RUN dotnet restore "HireConnect.Auth/HireConnect.Auth.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/HireConnect.Auth"
RUN dotnet publish "HireConnect.Auth.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HireConnect.Auth.dll"]
