# IP Intelligence

The IP Intelligence feature provides a unified view of IP address data by combining **MaxMind Insights** (geolocation, network traits, anonymizer flags) with **ProxyCheck.io** (proxy/VPN detection, risk scoring) into a single `IpIntelligenceDto` response.

## Architecture

```
GET /v1.1/lookup/intelligence/{hostname}
                │
                ▼
      IpIntelligenceService
       ┌────────┴────────┐
       ▼                 ▼          (parallel fan-out)
  MaxMind Insights   ProxyCheck.io
       │                 │
  Cache: geolocationsv11 │  Cache: proxycheck
  (7-day TTL)            │  (60-min TTL)
       │                 │
       └────────┬────────┘
                ▼
        IpIntelligenceDto
   (merged result + source status)
```

Both data sources are queried in parallel via `Task.WhenAll`. Each source follows a **cache-first** pattern — the cache is checked before making an external API call.

## Graceful Degradation

The service never fails silently. Every response includes source status metadata so consumers know exactly what data they received:

| Scenario | `MaxMindStatus` | `ProxyCheckStatus` | `IsPartial` | HTTP Status |
|---|---|---|---|---|
| Both succeed | `Success` | `Success` | `false` | 200 |
| MaxMind fails, ProxyCheck succeeds | `Failed` | `Success` | `true` | 200 |
| MaxMind succeeds, ProxyCheck fails | `Success` | `Failed` | `true` | 200 |
| Both fail | — | — | — | 503 |

The `SourceStatus` enum values are:

| Value | Meaning |
|---|---|
| `Success` | Data successfully retrieved (from cache or API) |
| `Failed` | Source was called but returned an error or timed out |
| `Unavailable` | Source is not configured or not available |

Consumers should check `IsPartial` and individual status fields to decide how to handle partial results. For example, a fraud-detection consumer might fall back to local heuristics when `ProxyCheckStatus` is `Failed`.

## Caching Strategy

| Source | Table | TTL | Config Key | Default |
|---|---|---|---|---|
| MaxMind Insights | `geolocationsv11` | Configurable | `Caching:InsightsCacheDays` | 7 days |
| ProxyCheck | `proxycheck` | Configurable | `Caching:ProxyCheckCacheMinutes` | 60 minutes |

- Both caches use Azure Table Storage with `UpsertEntityAsync` (Replace mode) for idempotent writes.
- Cache expiry is checked against the entity `Timestamp` — no explicit TTL column.
- Only successful API responses are cached. Failed lookups are not stored.
- The `DELETE /v1.1/lookup/{hostname}` endpoint removes entries from **all** cache tables (v1.0 `geolocations`, v1.1 `geolocationsv11`, and `proxycheck`).

## IpIntelligenceDto Field Reference

### Geo Context (from MaxMind Insights)

| Field | Type | Description |
|---|---|---|
| `Address` | `string` | Original IP/hostname requested |
| `TranslatedAddress` | `string` | Resolved IP address |
| `ContinentCode` | `string?` | Two-letter continent code (e.g. `NA`) |
| `ContinentName` | `string?` | Continent name |
| `CountryCode` | `string?` | ISO 3166-1 alpha-2 country code |
| `CountryName` | `string?` | Country name |
| `IsEuropeanUnion` | `bool` | Whether the country is in the EU |
| `CityName` | `string?` | City name |
| `PostalCode` | `string?` | Postal/ZIP code |
| `Subdivisions` | `List<string>` | State/province subdivisions |
| `Latitude` | `double?` | Latitude |
| `Longitude` | `double?` | Longitude |
| `AccuracyRadius` | `int?` | Accuracy radius in km |
| `Timezone` | `string?` | IANA timezone identifier |

### Network & Anonymizer (from MaxMind Insights)

| Field | Type | Description |
|---|---|---|
| `NetworkTraits` | `NetworkTraitsDto?` | ASN, ISP, organization, domain, connection type |
| `Anonymizer` | `AnonymizerDto?` | VPN, Tor, hosting provider, public proxy flags |

### Risk Assessment (from ProxyCheck.io)

| Field | Type | Description |
|---|---|---|
| `ProxyCheck` | `ProxyCheckDto?` | Nested object — `null` if ProxyCheck source failed |

`ProxyCheckDto` fields:

| Field | Type | Description |
|---|---|---|
| `Address` | `string` | Original IP/hostname |
| `TranslatedAddress` | `string` | Resolved IP address |
| `RiskScore` | `int` | Risk score 0–100 (higher = greater risk) |
| `IsProxy` | `bool` | Identified as a proxy |
| `IsVpn` | `bool` | Identified as a VPN |
| `ProxyType` | `string` | Connection type (`VPN`, `TOR`, `PROXY`, `DCH`, etc.) |
| `Country` | `string` | Country from ProxyCheck |
| `Region` | `string` | Region/state from ProxyCheck |
| `AsNumber` | `string` | Autonomous System Number |
| `AsOrganization` | `string` | Organization owning the ASN |

### Source Status Metadata

| Field | Type | Description |
|---|---|---|
| `MaxMindStatus` | `SourceStatus` | Status of the MaxMind Insights lookup |
| `ProxyCheckStatus` | `SourceStatus` | Status of the ProxyCheck.io lookup |
| `IsPartial` | `bool` | `true` if either source status is not `Success` |

## Endpoints

| Method | Path | Returns | Description |
|---|---|---|---|
| `GET` | `/v1.1/lookup/proxycheck/{hostname}` | `ProxyCheckDto` | Pure ProxyCheck.io risk assessment |
| `GET` | `/v1.1/lookup/intelligence/{hostname}` | `IpIntelligenceDto` | Merged MaxMind + ProxyCheck |
| `POST` | `/v1.1/lookup/intelligence` | `CollectionModel<IpIntelligenceDto>` | Batch (max 20, parallelism 5) |
| `DELETE` | `/v1.1/lookup/{hostname}` | — | Deletes from all cache tables |

## Configuration

### Required

| Key | Source | Description |
|---|---|---|
| `ProxyCheck:ApiKey` | Key Vault | ProxyCheck.io API key (manual step — see [manual-steps.md](manual-steps.md)) |

### Optional

| Key | Default | Description |
|---|---|---|
| `ProxyCheck:BaseUrl` | `https://proxycheck.io/v2/` | ProxyCheck API base URL |
| `Caching:ProxyCheckCacheMinutes` | `60` | ProxyCheck cache TTL in minutes |
| `Caching:InsightsCacheDays` | `7` | MaxMind Insights cache TTL in days |

## Infrastructure

The `proxycheck` Azure Storage table is provisioned by Terraform in `terraform/storage_account.tf` alongside the existing `geolocations` and `geolocationsv11` tables.

## Future: Portal-Web Migration

The `portal-web` application currently makes direct calls to the ProxyCheck.io API for IP risk assessment. Once the geo-location service's IP Intelligence endpoints are stable, `portal-web` should migrate to use `GetIpIntelligence` via the `MX.GeoLocation.Api.Client.V1` client instead. This provides:

- **Centralized caching** — avoids redundant ProxyCheck API calls across services
- **Unified data model** — geolocation + risk data in a single DTO
- **Graceful degradation** — automatic partial result handling without per-consumer retry logic
- **Single API key** — ProxyCheck credentials managed in one Key Vault instead of per-service
