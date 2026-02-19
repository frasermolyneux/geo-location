# GeoLocation Service

[![Build and Test](https://github.com/frasermolyneux/geo-location/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/frasermolyneux/geo-location/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/codequality.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/geo-location/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/copilot-setup-steps.yml)
[![Dependabot Automerge](https://github.com/frasermolyneux/geo-location/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/dependabot-automerge.yml)
[![Deploy Dev](https://github.com/frasermolyneux/geo-location/actions/workflows/deploy-dev.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/deploy-dev.yml)
[![Deploy Prd](https://github.com/frasermolyneux/geo-location/actions/workflows/deploy-prd.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/deploy-prd.yml)
[![Destroy Development](https://github.com/frasermolyneux/geo-location/actions/workflows/destroy-development.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/destroy-development.yml)
[![Destroy Environment](https://github.com/frasermolyneux/geo-location/actions/workflows/destroy-environment.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/destroy-environment.yml)
[![PR Verify](https://github.com/frasermolyneux/geo-location/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/pr-verify.yml)

## Documentation

* [API Versioning, APIM Routing & OpenAPI](docs/api-versioning-and-apim.md)
* [Testing with the GeoLocation API Client](docs/testing.md)
* [Manual Steps](docs/manual-steps.md)

## Overview

GeoLocation is a .NET 9 workload that fronts MaxMind lookups with an Entra-protected API and caches results in Azure Table Storage to reduce latency and cost. The API exposes two versioned endpoint groups:

- **v1.0**: Single/batch hostname/IP lookup, metadata deletion, and API info — cached permanently in the `geolocations` table
- **v1.1**: City and Insights lookups with typed DTOs and MaxMind Anonymizer support — cached in the `geolocationsv11` table (city: permanent, insights: configurable TTL)

Both versions enforce the `LookupApiUser` Entra role. An MVC web front end calls the API using API-key and Entra authentication, handles Cloudflare/X-Forwarded-For headers, and stores the user’s last lookup in session. The API serves its OpenAPI specs at runtime at `/openapi/v1.0.json` and `/openapi/v1.1.json`, and infrastructure is managed by Terraform under `terraform/`. Build versioning uses Nerdbank.GitVersioning.

## NuGet Packages

| Package | Description |
|---|---|
| `MX.GeoLocation.Abstractions.V1` | Interfaces and models for the GeoLocation API |
| `MX.GeoLocation.Api.Client.V1` | Typed HTTP client with DI registration via `AddGeoLocationApiClient()` |
| `MX.GeoLocation.Api.Client.Testing` | In-memory fakes and DTO factory helpers for consumer test projects — see [testing docs](docs/testing.md) |

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
