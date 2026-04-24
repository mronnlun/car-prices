# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Azure Function (.NET 10, isolated worker) that periodically scrapes and stores Volvo EX30 car listing prices from:
- **Blocket** (Swedish marketplace) — uses their internal mobility search API returning JSON
- **Nettiauto** (Finnish marketplace) — uses the public Nettix REST API (`api.nettix.fi`)

Data is stored in **Azure Table Storage** (partitioned by source, keyed by listing ID).

## Build & Run

```bash
dotnet build                    # Build
func start                     # Run locally (requires Azurite for Table Storage)
dotnet test                    # Run tests (when added)
az bicep build --file infra/main.bicep   # Validate Bicep
```

Requires `local.settings.json` with `AzureWebJobsStorage` (defaults to `UseDevelopmentStorage=true` for Azurite).

## Azure Naming Convention

`ProjectName-env-resourceabbr` — ProjectName is PascalCase without hyphens.

| Resource | Name |
|---|---|
| Resource Group | `CarPrices-{env}-rg` |
| Function App | `CarPrices-{env}-func` |
| App Service Plan | `CarPrices-{env}-asp` |
| Storage Account | `carprices{env}st` (lowercase, no hyphens) |
| Application Insights | `CarPrices-{env}-ai` |
| Log Analytics | `CarPrices-{env}-log` |

## Infrastructure & CI/CD

- `infra/main.bicep` — All Azure resources (Consumption plan Function App, Storage Account with CarListings table, App Insights, Log Analytics)
- `infra/main.bicepparam` — Parameter file (set environment here)
- `.github/workflows/ci.yml` — PR validation: build, test, Bicep lint
- `.github/workflows/cd.yml` — Deploy on push to main: infra (Bicep) then app (publish artifact)

CD uses workload identity federation (OIDC). Required GitHub secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`.

`host.json` limits concurrency to 1 function execution, and Bicep sets `functionAppScaleLimit: 1` to prevent multiple instances.

## Architecture

- `Functions/CarPriceScraperFunction.cs` — Timer-triggered (every 6 hours, `RunOnStartup=true`), orchestrates both scrapers in parallel
- `Services/NettiautoService.cs` — Fetches from Nettix REST API, maps to `CarListing`
- `Services/BlocketService.cs` — Fetches from Blocket mobility API, maps to `CarListing`
- `Services/CarListingStore.cs` — Upserts listings to Azure Table Storage, preserving `FirstSeenAt` for existing records
- `Models/CarListing.cs` — Table Storage entity (PartitionKey=source, RowKey=listing ID)
- `Models/NettiautoResponse.cs`, `BlocketResponse.cs` — API response DTOs

Services are registered via DI in `Program.cs` using typed `HttpClient` instances.

## Data Sources

- **Nettiauto API**: `https://api.nettix.fi/rest/car/search` — Swagger docs at `https://api.nettix.fi/docs/car/`
  - Requires OAuth2 client credentials (`NettiautoClientId`, `NettiautoClientSecret`) — token endpoint: `https://auth.nettix.fi/oauth2/token`
  - API returns mixed JSON types (ad `id` is string, option `id` is number) — deserialized with `JsonNumberHandling.AllowReadingFromString`
- **Blocket API**: Internal mobility search endpoint at `blocket.se/mobility/search/api/search/SEARCH_ID_CAR_USED`
  - Blocket may require a browser-like User-Agent header
  - Prices in SEK; mileage reported in Swedish miles (1 mil = 10 km)
