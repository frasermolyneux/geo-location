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
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _errorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentBag<string> _deletedAddresses = [];
    private readonly ConcurrentBag<string> _lookedUpAddresses = [];

    /// <summary>
    /// Registers a canned response for a specific address.
    /// </summary>
    public FakeGeoLookupApi AddResponse(string address, GeoLocationDto dto)
    {
        _responses[address] = dto;
        return this;
    }

    /// <summary>
    /// Registers a canned error response for a specific address.
    /// </summary>
    public FakeGeoLookupApi AddErrorResponse(string address, HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _errorResponses[address] = (statusCode, new ApiError(errorCode, errorMessage));
        return this;
    }

    /// <summary>
    /// Returns the set of addresses that had <see cref="DeleteMetadata"/> called on them.
    /// </summary>
    public IReadOnlyCollection<string> DeletedAddresses => _deletedAddresses.ToArray();

    /// <summary>
    /// Returns the set of addresses that were looked up via <see cref="GetGeoLocation"/> or <see cref="GetGeoLocations"/>.
    /// </summary>
    public IReadOnlyCollection<string> LookedUpAddresses => _lookedUpAddresses.ToArray();

    public Task<ApiResult<GeoLocationDto>> GetGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        _lookedUpAddresses.Add(hostname);

        if (_errorResponses.TryGetValue(hostname, out var error))
        {
            return Task.FromResult(new ApiResult<GeoLocationDto>(error.StatusCode,
                new ApiResponse<GeoLocationDto>(error.Error)));
        }

        var dto = _responses.GetValueOrDefault(hostname)
            ?? GeoLocationDtoFactory.CreateGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");

        return Task.FromResult(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(dto)));
    }

    public Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default)
    {
        foreach (var h in hostnames) _lookedUpAddresses.Add(h);

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
