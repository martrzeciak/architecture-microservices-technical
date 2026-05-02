FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["backend/OrderService/OrderService.csproj", "backend/OrderService/"]
COPY ["protos/", "protos/"]
RUN dotnet restore "backend/OrderService/OrderService.csproj"

COPY backend/OrderService/ backend/OrderService/
RUN dotnet publish "backend/OrderService/OrderService.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

EXPOSE 5003
EXPOSE 5004
EXPOSE 5005

ENTRYPOINT ["dotnet", "OrderService.dll"]
