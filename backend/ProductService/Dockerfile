FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/ProductService/ProductService.csproj", "backend/ProductService/"]
COPY ["protos/", "protos/"]
RUN dotnet restore "backend/ProductService/ProductService.csproj"

COPY backend/ProductService/ backend/ProductService/
RUN dotnet publish "backend/ProductService/ProductService.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

EXPOSE 5000
EXPOSE 5001
EXPOSE 5002

ENTRYPOINT ["dotnet", "ProductService.dll"]
