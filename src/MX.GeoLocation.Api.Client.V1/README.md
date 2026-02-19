# MX.GeoLocation.Api.Client.V1

Authenticated REST client for the GeoLocation lookup API. Provides DI registration, token management, retry policies, and versioned API access.

## Installation

```shell
dotnet add package MX.GeoLocation.Api.Client.V1
```

## Quick Start

### Register Services

```csharp
builder.Services.AddGeoLocationApiClient(options =>
{
    options.ConfigureBaseUrl("https://geolocation-api.example.com");
});
```

### Inject and Use

```csharp
public class MyService
{
    private readonly IGeoLocationApiClient _client;

    public MyService(IGeoLocationApiClient client)
    {
        _client = client;
    }

    public async Task<GeoLocationDto?> LookupAddress(string hostname)
    {
        var result = await _client.GeoLookup.V1.GetGeoLocation(hostname);

        if (result.IsSuccess)
            return result.Result;

        return null;
    }

    public async Task<CityGeoLocationDto?> LookupCityDetails(string hostname)
    {
        var result = await _client.GeoLookup.V1_1.GetCityGeoLocation(hostname);

        if (result.IsSuccess)
            return result.Result;

        return null;
    }
}
```

### Health and Info Endpoints

```csharp
var health = await _client.ApiHealth.CheckHealth();
var info = await _client.ApiInfo.GetApiInfo();
```

## API Surface

The `IGeoLocationApiClient` exposes:

| Property | Description |
|----------|-------------|
| `GeoLookup` | Versioned geo-lookup API (V1 and V1.1) |
| `ApiInfo` | API version and build information |
| `ApiHealth` | Health check endpoint |

## Testing

Use the companion package [`MX.GeoLocation.Api.Client.Testing`](https://www.nuget.org/packages/MX.GeoLocation.Api.Client.Testing) for in-memory fakes and test helpers.

## License

This project is licensed under the [GPL-3.0-only](https://spdx.org/licenses/GPL-3.0-only.html) license.
