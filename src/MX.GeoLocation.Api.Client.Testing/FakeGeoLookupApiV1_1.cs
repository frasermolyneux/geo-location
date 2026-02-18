using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models.V1_1;

using V1_1 = MX.GeoLocation.Abstractions.Interfaces.V1_1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// In-memory fake of <see cref="V1_1.IGeoLookupApi"/> for unit and integration tests.
/// Configure responses with <see cref="AddCityResponse"/> and <see cref="AddInsightsResponse"/>.
/// </summary>
public class FakeGeoLookupApiV1_1 : V1_1.IGeoLookupApi
{
    private readonly ConcurrentDictionary<string, CityGeoLocationDto> _cityResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, InsightsGeoLocationDto> _insightsResponses = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a canned city response for a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddCityResponse(string address, CityGeoLocationDto dto)
    {
        _cityResponses[address] = dto;
        return this;
    }

    /// <summary>
    /// Registers a canned insights response for a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddInsightsResponse(string address, InsightsGeoLocationDto dto)
    {
        _insightsResponses[address] = dto;
        return this;
    }

    public Task<ApiResult<CityGeoLocationDto>> GetCityGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        var dto = _cityResponses.GetValueOrDefault(hostname)
            ?? GeoLocationDtoFactory.CreateCityGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");

        return Task.FromResult(new ApiResult<CityGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<CityGeoLocationDto>(dto)));
    }

    public Task<ApiResult<InsightsGeoLocationDto>> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        var dto = _insightsResponses.GetValueOrDefault(hostname)
            ?? GeoLocationDtoFactory.CreateInsightsGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");

        return Task.FromResult(new ApiResult<InsightsGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<InsightsGeoLocationDto>(dto)));
    }
}
