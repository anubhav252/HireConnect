FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["HireConnect.Gateway/HireConnect.Gateway.csproj", "HireConnect.Gateway/"]
# Gateway might not need Shared, but let's check if it does.
# Actually, the template should be consistent.

RUN dotnet restore "HireConnect.Gateway/HireConnect.Gateway.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/HireConnect.Gateway"
RUN dotnet publish "HireConnect.Gateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HireConnect.Gateway.dll"]
