# MX.GeoLocation.LookupApi.Abstractions

Core abstractions library providing interfaces and models for the GeoLocation lookup service. This package defines the API contracts consumed by `MX.GeoLocation.Api.Client.V1` and tested with `MX.GeoLocation.Api.Client.Testing`.

## Installation

```shell
dotnet add package MX.GeoLocation.LookupApi.Abstractions
```

## Key Interfaces

### IGeoLookupApi (V1)

```csharp
public interface IGeoLookupApi
{
    Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname, CancellationToken cancellationToken = default);
    Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default);
}
```

### IGeoLookupApi (V1.1)

Provides typed responses with separate city and insights endpoints:

```csharp
public interface IGeoLookupApi
{
    Task<ApiResult<CityGeoLocationDto>> GetCityGeoLocation(string hostname, CancellationToken cancellationToken = default);
    Task<ApiResult<InsightsGeoLocationDto>> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken = default);
}
```

### IVersionedGeoLookupApi

Wraps both API versions for version-aware consumers:

```csharp
public interface IVersionedGeoLookupApi
{
    V1.IGeoLookupApi V1 { get; }
    V1_1.IGeoLookupApi V1_1 { get; }
}
```

## Data Models

| Model | Description |
|-------|-------------|
| `GeoLocationDto` | V1 response with address, country, city, coordinates, timezone |
| `CityGeoLocationDto` | V1.1 city-level data with subdivisions and network traits |
| `InsightsGeoLocationDto` | V1.1 extended data with anonymizer detection |
| `NetworkTraitsDto` | ASN, ISP, organization, user type metadata |
| `AnonymizerDto` | VPN, proxy, Tor exit node detection flags |
| `ApiInfoDto` | API version and build information |

## License

This project is licensed under the [GPL-3.0-only](https://spdx.org/licenses/GPL-3.0-only.html) license.
