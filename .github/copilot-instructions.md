# Copilot Instructions

## Architecture
- .NET 9 solution in `src/MX.GeoLocation.sln` with API (`MX.GeoLocation.Api.V1`) and MVC web (`MX.GeoLocation.Web`) projects plus abstractions and a generated API client.
- API uses MaxMind GeoIP2 and caches responses in Azure Table Storage:
  - **v1.0**: `geolocations` table with `GeoLocationTableEntity` (permanent cache)
  - **v1.1**: `geolocationsv11` table with `CityGeoLocationTableEntity` (city: permanent, insights: configurable TTL via `Caching:InsightsCacheDays`, default 7 days)
- API exposes two versioned endpoint groups:
  - **v1.0**: Single/batch lookup, metadata deletion, and API info (`/v1.0/lookup/...`, `/v1.0/info`)
  - **v1.1**: City and Insights lookups with typed DTOs (`/v1.1/lookup/city/...`, `/v1.1/lookup/insights/...`)
- Controllers are differentiated by namespace (`Controllers.V1`, `Controllers.V1_1`), both named `GeoLookupController`.
- API security is Entra ID via `Microsoft.Identity.Web`; the `LookupApiUser` role is required for controller access. The `/v1.0/info` and `/v1.0/health` endpoints are `[AllowAnonymous]`.
- OpenAPI specs are served at runtime at `/openapi/v1.0.json` and `/openapi/v1.1.json`. Scalar provides interactive API docs at `/scalar`.
- Web front end uses `MX.GeoLocation.Api.Client.V1` with API-key + Entra authentication, stores the last lookup in session, and respects `CF-Connecting-IP`/`X-Forwarded-For` headers (defaults to `8.8.8.8` in development).
- Application Insights telemetry is enabled with custom adaptive sampling (exceptions excluded) and Service Profiler in both API and Web.
- Build versioning uses Nerdbank.GitVersioning (`version.json` at repo root).

## Workflows
- Build: `dotnet build src/MX.GeoLocation.sln`
- Test: `dotnet test src/MX.GeoLocation.sln` (API/Web unit tests plus API client tests; Web integration tests are present). Test framework: xUnit + Moq + native assertions.
- OpenAPI spec is generated at runtime by the deployed app — no build-time generation or source-controlled spec files.
- APIM API definitions are imported via `az apim api import --specification-url` in the deploy workflows after the API App Service is deployed. Both v1.0 and v1.1 specs are imported separately with `--api-version` and `--api-version-set-id` to share the same version set.
- The APIM import uses `--service-url` with a version suffix (e.g. `/v1`, `/v1.1`) to bridge the version-free spec paths back to the versioned backend routes.
- Before importing specs, the workflow polls `GET /v1.0/info` on the App Service to verify the deployed build version matches the expected `build_version` from the CI job (Nerdbank.GitVersioning). This prevents importing stale specs during App Service restarts.
- No API-level policies are set in workflows; the Terraform product policy handles JWT validation, caching, and request forwarding.

## Configuration
- API needs `maxmind_userid`, `maxmind_apikey`, and `appdata_storage_connectionstring`; MaxMind secrets belong in Key Vault (see `docs/manual-steps.md`).
- Web needs `GeoLocationApi:BaseUrl`, `GeoLocationApi:ApiKey`, and `GeoLocationApi:ApplicationAudience`; user secrets are wired in `Program.cs` for development.
- Batch lookup caps at 20 entries; `localhost` and `127.0.0.1` are rejected to avoid local lookups.

## Key Files
- `MX.GeoLocation.Api.V1/Program.cs` sets API versioning (GroupNameFormat `'v'VV`), OpenAPI documents (`v1.0`, `v1.1`), auth, table storage, and health checks.
- `MX.GeoLocation.Api.V1/OpenApi/StripVersionPrefixTransformer.cs` strips version prefix (regex `^/v\d+(\.\d+)?`) from spec paths so APIM segment versioning can manage the version prefix without producing `/v1/v1/...` paths. Uses source-generated regex via `partial class` with `[GeneratedRegex]`.
- `MX.GeoLocation.Api.V1/OpenApi/BearerSecuritySchemeTransformer.cs` adds Bearer JWT security scheme to the OpenAPI document.
- `Controllers/V1/GeoLookupController.cs` implements GET/POST lookups and DELETE metadata with cache-first flow then MaxMind fallback.
- `Controllers/V1/ApiInfoController.cs` implements the `/v1.0/info` endpoint returning build version information (anonymous access).
- `Controllers/V1/HealthController.cs` implements the `/v1.0/health` endpoint wrapping the ASP.NET health check service (anonymous access).
- `Controllers/V1_1/GeoLookupController.cs` implements city and insights lookups with cache-first flow and configurable insights TTL.
- `Repositories/TableStorageGeoLocationRepository.cs` handles Azure Table persistence for both v1.0 (`geolocations`) and v1.1 (`geolocationsv11`) tables.
- `Repositories/MaxMindGeoLocationRepository.cs` wraps `MaxMind.GeoIP2.WebServiceClient` with dependency telemetry; provides `GetGeoLocation` (v1), `GetCityGeoLocation` and `GetInsightsGeoLocation` (v1.1).
- `Models/CityGeoLocationTableEntity.cs` is the table entity for v1.1 DTOs, serializing complex fields (Subdivisions, NetworkTraits, Anonymizer) as JSON columns.
- `MX.GeoLocation.Web/Program.cs` wires the API client and sessions; `HomeController.cs` drives lookup, batch lookup, and data removal flows.

## Infrastructure
- Terraform under `terraform/` builds App Services (API + Web on shared platform-hosting plan), API Management (Consumption), Key Vault, Storage (including both `geolocations` and `geolocationsv11` tables), DNS, Entra ID apps, and Application Insights (per-environment tfvars/backends). GitHub Actions workflows cover build/test, codequality, PR verify, deploy-dev/prd, destroy-development/environment, dependabot-automerge, and copilot-setup-steps.
- APIM API definitions are imported from the live deployed App Service via `az apim api import` in the GitHub Actions deploy workflows (not managed by Terraform). Terraform manages the APIM instance, version set (`geolocation-api`, scheme `Segment`), product, product policy (JWT/caching), and diagnostics.
- See [docs/api-versioning-and-apim.md](../docs/api-versioning-and-apim.md) for the full API versioning, APIM routing, and OpenAPI flow.
