using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// In-memory fake of <see cref="IGeoLookupApi"/> (V1) for unit and integration tests.
/// Configure responses with <see cref="AddResponse"/> before exercising the code under test.
/// Unconfigured addresses return a generic response with the requested address.
/// </summary>
public class FakeGeoLookupApi : IGeoLookupApi
{
    private readonly ConcurrentDictionary<string, GeoLocationDto> _responses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentBag<string> _deletedAddresses = [];

    /// <summary>
    /// Registers a canned response for a specific address.
    /// </summary>
    public FakeGeoLookupApi AddResponse(string address, GeoLocationDto dto)
    {
        _responses[address] = dto;
        return this;
    }

    /// <summary>
    /// Returns the set of addresses that had <see cref="DeleteMetadata"/> called on them.
    /// </summary>
    public IReadOnlyCollection<string> DeletedAddresses => _deletedAddresses.ToArray();

    public Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        var dto = _responses.GetValueOrDefault(hostname)
            ?? GeoLocationDtoFactory.CreateGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");

        return Task.FromResult(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(dto)));
    }

    public Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default)
    {
        var items = hostnames.Select(h =>
            _responses.GetValueOrDefault(h)
            ?? GeoLocationDtoFactory.CreateGeoLocation(address: h, cityName: "Test City", countryName: "Test Country"))
            .ToList();

        var collection = new CollectionModel<GeoLocationDto> { Items = items };
        return Task.FromResult(new ApiResult<CollectionModel<GeoLocationDto>>(
            HttpStatusCode.OK, new ApiResponse<CollectionModel<GeoLocationDto>>(collection)));
    }

    public Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default)
    {
        _deletedAddresses.Add(hostname);
        return Task.FromResult(new ApiResult(HttpStatusCode.OK, new ApiResponse()));
    }
}
