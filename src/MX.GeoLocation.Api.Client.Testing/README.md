# MX.GeoLocation.Api.Client.Testing

Test helpers for consumer applications that depend on the GeoLocation API client. Provides in-memory fakes of `IGeoLocationApiClient`, DTO factory methods, and DI extensions for integration tests.

## Installation

```shell
dotnet add package MX.GeoLocation.Api.Client.Testing
```

## Quick Start — Integration Tests

Replace the real client with fakes in your test DI container:

```csharp
services.AddFakeGeoLocationApiClient(client =>
{
    client.V1Lookup.AddResponse("8.8.8.8",
        GeoLocationDtoFactory.CreateGeoLocation(
            address: "8.8.8.8",
            cityName: "Mountain View",
            countryName: "United States"));
});
```

## Quick Start — Unit Tests

Create and configure the fake client directly:

```csharp
var fakeClient = new FakeGeoLocationApiClient();

fakeClient.V1Lookup.AddResponse("1.1.1.1",
    GeoLocationDtoFactory.CreateGeoLocation(cityName: "San Francisco"));

fakeClient.V1_1Lookup.AddCityResponse("1.1.1.1",
    GeoLocationDtoFactory.CreateCityGeoLocation(cityName: "San Francisco"));

var sut = new MyService(fakeClient);
var result = await sut.LookupAddress("1.1.1.1");

Assert.Equal("San Francisco", result?.CityName);
```

## DTO Factories

`GeoLocationDtoFactory` provides static factory methods with sensible defaults:

```csharp
GeoLocationDtoFactory.CreateGeoLocation(address: "10.0.0.1", cityName: "London");
GeoLocationDtoFactory.CreateCityGeoLocation(countryName: "UK");
GeoLocationDtoFactory.CreateInsightsGeoLocation(cityName: "Berlin");
GeoLocationDtoFactory.CreateNetworkTraits(isp: "Cloudflare");
GeoLocationDtoFactory.CreateAnonymizer(isAnonymousVpn: true);
GeoLocationDtoFactory.CreateApiInfo(version: "1.0.0");
```

## Configuring Error Responses

```csharp
fakeClient.V1Lookup.AddErrorResponse("invalid-host",
    HttpStatusCode.NotFound, "NotFound", "Address not found");

fakeClient.V1Lookup.SetDefaultBehavior(DefaultLookupBehavior.ReturnError);
```

## Tracking Calls

The fake APIs expose read-only collections to verify interactions:

```csharp
Assert.Contains("8.8.8.8", fakeClient.V1Lookup.LookedUpAddresses);
Assert.Contains("old-record", fakeClient.V1Lookup.DeletedAddresses);
Assert.Contains("1.1.1.1", fakeClient.V1_1Lookup.CityLookedUpAddresses);
```

## Resetting State Between Tests

```csharp
fakeClient.Reset(); // Clears all configured responses and tracked calls
```

## License

This project is licensed under the [GPL-3.0-only](https://spdx.org/licenses/GPL-3.0-only.html) license.
