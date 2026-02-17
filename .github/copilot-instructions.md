# Copilot Instructions

## Architecture
- .NET 9 solution in `src/MX.GeoLocation.sln` with API (`MX.GeoLocation.Api.V1`) and MVC web (`MX.GeoLocation.Web`) projects plus abstractions and a generated API client.
- API uses MaxMind GeoIP2 and caches responses in Azure Table Storage (partition key `addresses`, row key = translated address); exposes versioned endpoints for single/batch lookup and metadata deletion; health at `/health`.
- API security is Entra ID via `Microsoft.Identity.Web`; the `LookupApiUser` role is required for controller access.
- OpenAPI spec is served at runtime via ASP.NET Core 9 built-in `AddOpenApi()` / `MapOpenApi()` at `/openapi/v1.json` (all environments). Scalar provides interactive API docs at `/scalar`.
- Web front end uses `MX.GeoLocation.Api.Client.V1` with API-key + Entra authentication, stores the last lookup in session, and respects `CF-Connecting-IP`/`X-Forwarded-For` headers (defaults to `8.8.8.8` in development).
- Application Insights telemetry is enabled with custom adaptive sampling (exceptions excluded) and Service Profiler in both API and Web.

## Workflows
- Build: `dotnet build src/MX.GeoLocation.sln`
- Test: `dotnet test src/MX.GeoLocation.sln` (API/Web unit tests plus API client tests; Web integration tests are present)
- OpenAPI spec is generated at runtime by the deployed app — no build-time generation or source-controlled spec files.
- APIM API definition is imported via `az apim api import --specification-url` in the deploy workflows after the API App Service is deployed.

## Configuration
- API needs `maxmind_userid`, `maxmind_apikey`, and `appdata_storage_connectionstring`; MaxMind secrets belong in Key Vault (see `docs/manual-steps.md`).
- Web needs `GeoLocationApi:BaseUrl`, `GeoLocationApi:ApiKey`, and `GeoLocationApi:ApplicationAudience`; user secrets are wired in `Program.cs` for development.
- Batch lookup caps at 20 entries; `localhost` and `127.0.0.1` are rejected to avoid local lookups.

## Key Files
- `MX.GeoLocation.Api.V1/Program.cs` sets API versioning, OpenAPI, auth, caching, and health checks.
- `MX.GeoLocation.Api.V1/OpenApi/StripVersionPrefixTransformer.cs` strips `/v1/` from spec paths so APIM segment versioning can manage the version prefix.
- `MX.GeoLocation.Api.V1/OpenApi/BearerSecuritySchemeTransformer.cs` adds Bearer JWT security scheme to the OpenAPI document.
- `Controllers/GeoLookupController.cs` implements GET/POST lookups and DELETE metadata with cache-first flow then MaxMind fallback; route is `v{version:apiVersion}`.
- `Repositories/TableStorageGeoLocationRepository.cs` handles Azure Table persistence; `MaxMindGeoLocationRepository.cs` wraps `MaxMind.GeoIP2.WebServiceClient` with dependency telemetry.
- `MX.GeoLocation.Web/Program.cs` wires the API client and sessions; `HomeController.cs` drives lookup, batch lookup, and data removal flows.

## Infrastructure
- Terraform under `terraform/` builds App Services (API + Web on shared platform-hosting plan), API Management (Consumption), Key Vault, Storage, DNS, Entra ID apps, and Application Insights (per-environment tfvars/backends). GitHub Actions workflows cover build/test, codequality, PR verify, deploy-dev/prd, destroy-development/environment, dependabot-automerge, and copilot-setup-steps.
- APIM API definitions are imported from the live deployed App Service via `az apim api import` in the GitHub Actions deploy workflows (not managed by Terraform). Terraform manages the APIM instance, backend, version set, product, and policies.
