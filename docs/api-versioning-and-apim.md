# API Versioning, APIM Routing & OpenAPI

This document describes how API versioning, path routing, OpenAPI spec generation, and APIM integration work together in the GeoLocation service.

## Backend (ASP.NET Core 9)

The API uses `Asp.Versioning` with URL segment versioning:

- **Controller route**: `[Route("v{version:apiVersion}")]` + action routes like `[Route("lookup/{hostname}")]`
- **Actual backend paths**: `/v1/lookup/{hostname}` (GET/DELETE), `/v1/lookup` (POST)
- **Version reader**: `UrlSegmentApiVersionReader` extracts the version from the URL path
- **API Explorer**: `SubstituteApiVersionInUrl = true` replaces the `{version}` placeholder with `v1` in the generated OpenAPI spec

## OpenAPI Spec Generation

The OpenAPI spec is served at runtime at `/openapi/v1.json` (all environments). Two document transformers modify the spec before it is served:

1. **`StripVersionPrefixTransformer`** — Removes the `/v1` prefix from all spec paths (e.g. `/v1/lookup/{hostname}` → `/lookup/{hostname}`). This is required because APIM segment versioning manages the version prefix itself; without stripping, APIM would produce double-versioned paths (`/v1/v1/lookup/...`).

2. **`BearerSecuritySchemeTransformer`** — Adds the Bearer JWT security scheme and applies it to all operations.

Scalar provides interactive API docs at `/scalar`.

## APIM Configuration

### Terraform-managed resources

- **APIM instance**: Consumption tier
- **Version set**: `geolocation-api` with `Segment` versioning scheme — APIM manages the `/v1` segment in the consumer-facing URL
- **Product**: `geolocation-api` with subscription required
- **Product policy**: JWT validation (Entra ID), response caching (3600s), and request forwarding

### Workflow-managed resources (GitHub Actions)

The API definition is imported via `az apim api import` after the App Service is deployed:

- `--specification-url`: Points to the live app's `/openapi/v1.json` (paths are version-free after transformer)
- `--path geolocation`: Sets the APIM API prefix in the consumer-facing URL
- `--api-id geolocation-api-v1`: Links the API to the version set with version `v1`
- `--service-url .../v1`: Includes `/v1` so APIM reconstructs the full backend path the controller expects

The API is then added to the product for subscription key access.

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

## API Client

The `MX.GeoLocation.Api.Client.V1` constructs relative paths with the version prefix:

- `v1/lookup/{hostname}` (GET)
- `v1/lookup` (POST)
- `v1/lookup/{hostname}` (DELETE)

These are appended to a configured `BaseUrl`:

- **Direct to App Service**: `BaseUrl` = `https://{app-service}.azurewebsites.net` → `/v1/lookup/...` ✅
- **Through APIM**: `BaseUrl` = `https://{apim-gateway}/geolocation` → `/geolocation/v1/lookup/...` ✅

## Key Design Decisions

1. **Spec paths are version-free** — The `StripVersionPrefixTransformer` removes `/v1` from the OpenAPI spec so APIM segment versioning can own the version prefix without duplication.

2. **Service URL includes `/v1`** — Because the spec paths are version-free but the backend controller still expects `/v1/...`, the APIM service URL must include `/v1` to bridge the gap.

3. **No API-level policies in workflows** — The Terraform product policy handles JWT validation, caching, and forwarding. The workflow only imports the spec, sets the service URL, and assigns the API to the product.

4. **Runtime spec generation** — No build-time generation or source-controlled spec files. The deployed app serves its own spec, and the workflow imports it directly from the live URL.
