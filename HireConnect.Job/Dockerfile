FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["HireConnect.Job/HireConnect.Job.csproj", "HireConnect.Job/"]
COPY ["HireConnect.Shared/HireConnect.Shared.csproj", "HireConnect.Shared/"]

RUN dotnet restore "HireConnect.Job/HireConnect.Job.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/HireConnect.Job"
RUN dotnet publish "HireConnect.Job.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HireConnect.Job.dll"]
