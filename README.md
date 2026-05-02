# eShop — Angular 20 + .NET 10 + gRPC

Implementacja testowa dla pracy magisterskiej: *Architektura mikroserwisów w ekosystemie .NET*.  
Porównanie wydajności **gRPC vs REST** (rozdziały 6–7 tezy).

## Stack

| Warstwa | Technologia |
|---|---|
| Backend | .NET 10 LTS, ASP.NET Core, Grpc.AspNetCore, EF Core 10 |
| Frontend | Angular 20, @ngx-grpc, Signals API, HttpClient |
| Proxy | Envoy Proxy (translacja gRPC-Web → gRPC) |
| Baza danych | PostgreSQL 17 |
| Monitoring | Prometheus + Grafana |
| Testy | k6 |

## Architektura

```
Angular 20 → @ngx-grpc → Envoy :8080 → .NET 10 gRPC :5001/:5004
Angular 20 → HttpClient → .NET 10 REST :5000/api / :5003/api
```

## Wymagania wstępne

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (uruchomiony)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — tylko do uruchomienia frontendu i testów k6 lokalnie
- [Node.js 22 LTS](https://nodejs.org/) — tylko do frontendu
- [k6](https://k6.io/docs/getting-started/installation/) — tylko do testów obciążeniowych

---

## Uruchomienie backendu

Wszystkie serwisy backendowe uruchamiane są przez Docker Compose z katalogu projektu:

```powershell
# Z katalogu architecture-microservices-technical/
docker compose up -d
```

Przy pierwszym uruchomieniu Docker pobierze obrazy bazowe i zbuduje serwisy aplikacji.  
Migracje bazy danych są stosowane automatycznie przy starcie każdego serwisu.

### Zatrzymanie

```powershell
docker compose down
```

Aby usunąć również dane bazy:

```powershell
docker compose down -v
```

### Przebudowanie po zmianach w kodzie

```powershell
docker compose build --no-cache product-service order-service
docker compose up -d
```

---

## Dostępne adresy po uruchomieniu

| Serwis | Protokół | Adres |
|---|---|---|
| ProductService REST | HTTP | http://localhost:5000/api/products |
| ProductService gRPC | HTTP/2 | localhost:5001 |
| OrderService REST | HTTP | http://localhost:5003/api/orders |
| OrderService gRPC | HTTP/2 | localhost:5004 |
| Envoy (gRPC-Web) | HTTP | http://localhost:8080 |
| Prometheus | HTTP | http://localhost:9090 |
| Grafana | HTTP | http://localhost:3000 (admin / admin) |
| PostgreSQL | TCP | localhost:5432 |

### Health checks

```powershell
curl http://localhost:5000/healthz/live   # ProductService
curl http://localhost:5003/healthz/live   # OrderService
```

---

## Uruchomienie frontendu (Angular)

```powershell
cd frontend/angular-app
npm ci
ng serve
# Otwórz http://localhost:4200
```

---

## Testy obciążeniowe (k6)

Scenariusze testowe znajdują się w katalogu `tests/k6/`.

```powershell
# Przykład: test gRPC vs REST dla produktów
k6 run tests/k6/<nazwa_scenariusza>.js
```

---

## Struktura projektu

```
architecture-microservices-technical/
  docker-compose.yml           # Główny plik uruchomieniowy
  protos/                      # Wspólne definicje .proto (API-first)
    product.proto
    order.proto
  backend/
    ProductService/            # ASP.NET Core: gRPC + REST, porty 5000-5002
    OrderService/              # ASP.NET Core: gRPC + REST, porty 5003-5005
  frontend/
    angular-app/               # Angular 20 + @ngx-grpc
  infrastructure/
    envoy/envoy.yaml           # gRPC-Web → gRPC proxy
    prometheus/                # Konfiguracja scrapowania metryk
    grafana/                   # Dashboardy i provisioning
    postgres/                  # Skrypt inicjalizacji baz danych
    certs/                     # Certyfikaty TLS dla gRPC
  tests/
    k6/                        # Scenariusze testów obciążeniowych
```
