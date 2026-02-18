# API Versioning, APIM Routing & OpenAPI

This document describes how API versioning, path routing, OpenAPI spec generation, APIM integration, and deployment version verification work together in the GeoLocation service.

## Backend (ASP.NET Core 9)

The API uses `Asp.Versioning` with URL segment versioning:

- **Controller routes**: `[Route("v{version:apiVersion}")]` + action routes like `[Route("lookup/{hostname}")]`
- **API versions**:
  - **v1.0**: Single/batch IP lookup and metadata deletion (`/v1.0/lookup/...`, `/v1.0/info`)
  - **v1.1**: City and Insights lookups with typed DTOs (`/v1.1/lookup/city/...`, `/v1.1/lookup/insights/...`)
- **Version reader**: `UrlSegmentApiVersionReader` extracts the version from the URL path
- **Group name format**: `'v'VV` — always includes the minor version (`v1.0`, `v1.1`) to ensure unambiguous OpenAPI document grouping
- **Controllers**: Both versions use `GeoLookupController` differentiated by namespace (`Controllers.V1`, `Controllers.V1_1`)

## OpenAPI Spec Generation

Two separate OpenAPI documents are served at runtime:

- `/openapi/v1.0.json` — v1.0 endpoints (lookup, batch lookup, delete, info)
- `/openapi/v1.1.json` — v1.1 endpoints (city lookup, insights lookup)

Two document transformers modify each spec:

1. **`StripVersionPrefixTransformer`** — Uses a regex (`^/v\d+(\.\d+)?`) to remove the version prefix from all spec paths (e.g. `/v1.0/lookup/{hostname}` → `/lookup/{hostname}`). This is required because APIM segment versioning manages the version prefix itself; without stripping, APIM would produce double-versioned paths (`/v1/v1/lookup/...`).

2. **`BearerSecuritySchemeTransformer`** — Adds the Bearer JWT security scheme and applies it to all operations.

Scalar provides interactive API docs at `/scalar`.

## Build Versioning

The project uses **Nerdbank.GitVersioning** (`version.json` at repo root) for deterministic versioning:

- Assembly `InformationalVersion` is stamped at build time with the full SemVer2 string (e.g. `1.0.3+abc123def`)
- The `dotnet-web-ci` action exposes `build_version` (NuGet version, e.g. `1.0.3`) as a job output
- The `/v1.0/info` endpoint returns the running version for deployment verification

## APIM Configuration

### Terraform-managed resources

- **APIM instance**: Consumption tier
- **Version set**: `geolocation-api` with `Segment` versioning scheme — APIM manages the version segment (e.g. `/v1`, `/v1.1`) in the consumer-facing URL
- **Product**: `geolocation-api` with subscription required
- **Product policy**: JWT validation (Entra ID), response caching (3600s), and request forwarding

### Workflow-managed resources (GitHub Actions)

The API definitions are imported via `az apim api import` after the App Service is deployed. Both v1 and v1.1 specs are imported:

| Parameter | v1 | v1.1 |
|---|---|---|
| `--api-id` | `geolocation-api-v1` | `geolocation-api-v1-1` |
| `--api-version` | `v1` | `v1.1` |
| `--api-version-set-id` | `geolocation-api` | `geolocation-api` |
| `--specification-url` | `.../openapi/v1.0.json` | `.../openapi/v1.1.json` |
| `--service-url` | `.../v1` | `.../v1.1` |
| `--path` | `geolocation` | `geolocation` |

Both APIs share the same `--path` and version set — APIM requires this for segment versioning to work.

Each API is then added to the product for subscription key access.

## Deployment Version Verification

Before importing OpenAPI specs, the workflow verifies the deployed app is running the expected build:

1. The `build-and-test` job outputs `build_version` (from Nerdbank.GitVersioning via the `dotnet-web-ci` action)
2. The `apim-api-import` job polls `GET /v1.0/info` on the App Service, comparing `.buildVersion` from the response to the expected version
3. Polling runs up to 30 attempts with 10-second intervals (5 minutes max)
4. The APIM spec import only proceeds once the version matches

This prevents importing a stale OpenAPI spec from a previous deployment that hasn't finished recycling.

## Request Flow

```
Consumer request:
  GET https://{apim-gateway}/geolocation/v1/lookup/76.198.236.230
                             │            │  │
                             │            │  └─ Operation path (matched from spec: /lookup/{hostname})
                             │            └──── Version segment (managed by APIM segment versioning)
                             └───────────────── API path (--path geolocation)

APIM forwards to backend:
  GET https://{app-service}.azurewebsites.net/v1/lookup/76.198.236.230
                                             │   │
                                             │   └─ Operation path from spec
                                             └──── From --service-url (includes /v1)

Backend matches:
  [Route("v{version:apiVersion}")] + [Route("lookup/{hostname}")] ✅
```

The same flow applies to v1.1 with `/v1.1/` in the version segment and service URL.

## Table Storage Caching

- **v1.0**: Uses `geolocations` table with `GeoLocationTableEntity` — permanent cache
- **v1.1**: Uses `geolocationsv11` table with `CityGeoLocationTableEntity`:
  - **City lookups**: Permanent cache (no TTL)
  - **Insights lookups**: Configurable TTL via `Caching:InsightsCacheDays` (default 7 days), checked against entity `Timestamp`

Both use `UpsertEntityAsync` with `TableUpdateMode.Replace` for idempotent writes.

## API Client

The `MX.GeoLocation.Api.Client.V1` provides typed access via `IGeoLocationApiClient`:

- `client.GeoLookup.V1` — v1.0 endpoints (`GetGeoLocation`, `GetGeoLocations`, `DeleteMetadata`)
- `client.GeoLookup.V1_1` — v1.1 endpoints (`GetCityGeoLocation`, `GetInsightsGeoLocation`)
- `client.ApiInfo` — version info endpoint (`GetApiInfo`)

Client paths include the version prefix (e.g. `v1/lookup/{hostname}`, `v1.1/lookup/city/{hostname}`, `v1/info`):

- **Direct to App Service**: `BaseUrl` = `https://{app-service}.azurewebsites.net` → `/v1/lookup/...` ✅
- **Through APIM**: `BaseUrl` = `https://{apim-gateway}/geolocation` → `/geolocation/v1/lookup/...` ✅

### Testing

The `MX.GeoLocation.Api.Client.Testing` package provides in-memory fakes (`FakeGeoLocationApiClient`) and DTO factory methods (`GeoLocationDtoFactory`) so consumer apps can test against the client without mocking frameworks. See [testing docs](testing.md) for full usage examples.

## Key Design Decisions

1. **Spec paths are version-free** — The `StripVersionPrefixTransformer` removes the version prefix from the OpenAPI spec so APIM segment versioning can own the version prefix without duplication.

2. **Service URL includes the version** — Because the spec paths are version-free but the backend controllers still expect `/v1.0/...` or `/v1.1/...`, the APIM service URL must include the version suffix to bridge the gap.

3. **Group name format uses `'v'VV`** — Always includes the minor version (`v1.0`, `v1.1`) to prevent ambiguous prefix matching. Using `'v'V` would produce `v1` for version 1.0 which prefix-matches both `v1` and `v1.1` documents.

4. **No API-level policies in workflows** — The Terraform product policy handles JWT validation, caching, and forwarding. The workflow only imports the spec, sets the service URL, and assigns the API to the product.

5. **Runtime spec generation** — No build-time generation or source-controlled spec files. The deployed app serves its own spec, and the workflow imports it directly from the live URL.

6. **Version verification before import** — The workflow polls the `/v1.0/info` endpoint to confirm the correct build is running before importing specs, preventing stale spec imports during App Service restarts.
