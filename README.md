# GeoLocation Service

[![Code Quality](https://github.com/frasermolyneux/geo-location/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/codequality.yml)
[![Copilot Setup Steps](https://github.com/frasermolyneux/geo-location/actions/workflows/copilot-setup-steps.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/copilot-setup-steps.yml)
[![Dependabot Automerge](https://github.com/frasermolyneux/geo-location/actions/workflows/dependabot-automerge.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/dependabot-automerge.yml)
[![Dependency Review](https://github.com/frasermolyneux/geo-location/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/frasermolyneux/geo-location/actions/workflows/dependency-review.yml)

## Documentation

* [Manual Steps](/docs/manual-steps.md) - Key Vault entries for MaxMind credentials.

## Overview

GeoLocation is a .NET 9 workload that fronts MaxMind lookups with an Entra-protected API and caches results in Azure Table Storage to reduce latency and cost. The API exposes versioned endpoints for single or batch hostname/IP lookups plus metadata deletion, enforcing the `LookupApiUser` role. An MVC web front end calls the API using API-key and Entra authentication, handles Cloudflare/X-Forwarded-For headers, and stores the user’s last lookup in session. Debug builds generate OpenAPI output under `openapi/`, and infrastructure/Bicep assets live in the `bicep/` and `params/` folders.

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
