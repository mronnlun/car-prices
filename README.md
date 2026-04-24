# Car Prices

Azure Function that periodically scrapes Volvo EX30 car listing prices from Swedish and Finnish marketplaces and stores them in Azure Table Storage.

## Data Sources

| Source | Market | API | Currency |
|---|---|---|---|
| [Blocket](https://www.blocket.se) | Sweden | Internal mobility search API (JSON) | SEK |
| [Nettiauto](https://www.nettiauto.com) | Finland | [Nettix REST API](https://api.nettix.fi/docs/car/) (OAuth2) | EUR |

## Tech Stack

- .NET 10 (isolated worker)
- Azure Functions v4 (timer-triggered, every 6 hours)
- Azure Table Storage
- Application Insights
- Bicep (infrastructure as code)
- GitHub Actions (CI/CD)

## Local Development

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), [Azure Functions Core Tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local), [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite)

```bash
dotnet build        # Build
func start          # Run locally (Azurite must be running)
dotnet test         # Run tests
```

`local.settings.json` is gitignored and must include:
- `AzureWebJobsStorage` — defaults to `UseDevelopmentStorage=true` for Azurite
- `NettiautoClientId` — Nettix API client ID
- `NettiautoClientSecret` — Nettix API client secret

The function runs on startup (`RunOnStartup=true`) and then every 6 hours. Concurrency is limited to 1 execution on 1 instance via `host.json` and Bicep `functionAppScaleLimit`.

## Azure Resources

All resources follow the naming convention `CarPrices-{env}-{abbreviation}` (PascalCase, no hyphens in project name).

| Resource | Name | Purpose |
|---|---|---|
| Resource Group | `CarPrices-{env}-rg` | Container for all resources |
| Function App | `CarPrices-{env}-func` | Runs the scraper |
| App Service Plan | `CarPrices-{env}-asp` | Consumption (serverless) plan |
| Storage Account | `carprices{env}st` | Table Storage + Functions runtime |
| Application Insights | `CarPrices-{env}-ai` | Monitoring and logging |
| Log Analytics | `CarPrices-{env}-log` | Log sink for App Insights |

Infrastructure is defined in `infra/main.bicep` and parameterized via `infra/main.bicepparam`.

## CI/CD

| Workflow | Trigger | Steps |
|---|---|---|
| CI (`.github/workflows/ci.yml`) | PR to `main` | Build, test, validate Bicep |
| CD (`.github/workflows/cd.yml`) | Push to `main` | Deploy infra (Bicep), then deploy app |

CD authenticates via workload identity federation (OIDC). Required GitHub secrets:
- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

Nettiauto credentials are passed as secure Bicep parameters during deployment.

## Project Structure

```
├── Functions/          # Azure Function entry points
├── Models/             # Table Storage entities and API response DTOs
├── Services/           # Data fetching and storage services
├── infra/              # Bicep infrastructure templates
├── .github/workflows/  # CI/CD pipelines
└── Program.cs          # DI and host configuration
```
