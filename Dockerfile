# STAGE 1: BUILD
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only csproj first (for caching)
COPY MyShop.csproj .
RUN dotnet restore

# Copy everything
COPY . .

# Publish ONLY project (not solution)
RUN dotnet publish MyShop.csproj -c Release -o /app/publish

# STAGE 2: RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyShop.dll"]