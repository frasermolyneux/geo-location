# Copilot Instructions

## Architecture
- .NET 9 solution in `src/MX.GeoLocation.sln` with API (`MX.GeoLocation.Api.V1`) and MVC web (`MX.GeoLocation.Web`) projects plus abstractions and a generated API client.
- API uses MaxMind GeoIP2 and caches responses in Azure Table Storage (partition key `addresses`, row key = translated address); exposes versioned endpoints for single/batch lookup and metadata deletion; health at `/api/health`.
- API security is Entra ID via `Microsoft.Identity.Web`; the `LookupApiUser` role is required for controller access. Debug builds generate `openapi/openapi-v1.json` via the post-build `dotnet swagger` target.
- Web front end uses `MX.GeoLocation.Api.Client.V1` with API-key + Entra authentication, stores the last lookup in session, and respects `CF-Connecting-IP`/`X-Forwarded-For` headers (defaults to `8.8.8.8` in development).
- Application Insights telemetry is enabled with custom adaptive sampling (exceptions excluded) and Service Profiler in both API and Web.

## Workflows
- Build: `dotnet build src/MX.GeoLocation.sln`
- Test: `dotnet test src/MX.GeoLocation.sln` (API/Web unit tests plus API client tests; Web integration tests are present)
- Debug builds emit OpenAPI; `dotnet tool restore` is required for the swagger CLI when the post-build target runs.

## Configuration
- API needs `maxmind_userid`, `maxmind_apikey`, and `appdata_storage_connectionstring`; MaxMind secrets belong in Key Vault (see `docs/manual-steps.md`).
- Web needs `GeoLocationApi:BaseUrl`, `GeoLocationApi:ApiKey`, and `GeoLocationApi:ApplicationAudience`; user secrets are wired in `Program.cs` for development.
- Batch lookup caps at 20 entries; `localhost` and `127.0.0.1` are rejected to avoid local lookups.

## Key Files
- `MX.GeoLocation.Api.V1/Program.cs` sets API versioning, Swagger, auth, caching, and health checks.
- `Controllers/GeoLookupController.cs` implements GET/POST lookups and DELETE metadata with cache-first flow then MaxMind fallback.
- `Repositories/TableStorageGeoLocationRepository.cs` handles Azure Table persistence; `MaxMindGeoLocationRepository.cs` wraps `MaxMind.GeoIP2.WebServiceClient` with dependency telemetry.
- `MX.GeoLocation.Web/Program.cs` wires the API client and sessions; `HomeController.cs` drives lookup, batch lookup, and data removal flows.

## Infrastructure
- Bicep templates and parameter files live under `bicep/` and `params/`; generated OpenAPI artifacts land in `openapi/`.
