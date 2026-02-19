using System.Collections.Concurrent;
using System.Net;
using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Interfaces.V1;
using MX.GeoLocation.Abstractions.Models.V1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// Controls how the fake responds to addresses that have no explicit canned response.
/// </summary>
public enum DefaultLookupBehavior
{
    /// <summary>
    /// Return a generic success response with "Test City" / "Test Country" (default).
    /// </summary>
    ReturnGenericSuccess,

    /// <summary>
    /// Return an error response for unconfigured addresses.
    /// </summary>
    ReturnError
}

/// <summary>
/// In-memory fake of <see cref="IGeoLookupApi"/> (V1) for unit and integration tests.
/// Configure responses with <see cref="AddResponse"/> before exercising the code under test.
/// Unconfigured addresses return a generic response with the requested address by default;
/// use <see cref="SetDefaultBehavior"/> to change this.
/// </summary>
public class FakeGeoLookupApi : IGeoLookupApi
{
    private readonly ConcurrentDictionary<string, GeoLocationDto> _responses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _errorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentBag<string> _deletedAddresses = [];
    private readonly ConcurrentBag<string> _lookedUpAddresses = [];
    private DefaultLookupBehavior _defaultBehavior = DefaultLookupBehavior.ReturnGenericSuccess;
    private HttpStatusCode _defaultErrorStatusCode = HttpStatusCode.NotFound;
    private string _defaultErrorCode = "NOT_FOUND";
    private string _defaultErrorMessage = "Address not configured in fake";

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
    /// Configures how unconfigured addresses are handled.
    /// When set to <see cref="DefaultLookupBehavior.ReturnError"/>, lookups for addresses
    /// without a canned response will return an error instead of a generic success.
    /// </summary>
    public FakeGeoLookupApi SetDefaultBehavior(
        DefaultLookupBehavior behavior,
        HttpStatusCode errorStatusCode = HttpStatusCode.NotFound,
        string errorCode = "NOT_FOUND",
        string errorMessage = "Address not configured in fake")
    {
        _defaultBehavior = behavior;
        _defaultErrorStatusCode = errorStatusCode;
        _defaultErrorCode = errorCode;
        _defaultErrorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Clears all configured responses, error responses, and tracking state.
    /// Resets default behavior to <see cref="DefaultLookupBehavior.ReturnGenericSuccess"/>.
    /// </summary>
    public FakeGeoLookupApi Reset()
    {
        _responses.Clear();
        _errorResponses.Clear();
        while (_deletedAddresses.TryTake(out _)) { }
        while (_lookedUpAddresses.TryTake(out _)) { }
        _defaultBehavior = DefaultLookupBehavior.ReturnGenericSuccess;
        _defaultErrorStatusCode = HttpStatusCode.NotFound;
        _defaultErrorCode = "NOT_FOUND";
        _defaultErrorMessage = "Address not configured in fake";
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

        if (_responses.TryGetValue(hostname, out var dto))
        {
            return Task.FromResult(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(dto)));
        }

        if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
        {
            return Task.FromResult(new ApiResult<GeoLocationDto>(_defaultErrorStatusCode,
                new ApiResponse<GeoLocationDto>(new ApiError(_defaultErrorCode, _defaultErrorMessage))));
        }

        dto = GeoLocationDtoFactory.CreateGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");
        return Task.FromResult(new ApiResult<GeoLocationDto>(HttpStatusCode.OK, new ApiResponse<GeoLocationDto>(dto)));
    }

    public Task<ApiResult<CollectionModel<GeoLocationDto>>> GetGeoLocations(List<string> hostnames, CancellationToken cancellationToken = default)
    {
        foreach (var h in hostnames) _lookedUpAddresses.Add(h);

        List<GeoLocationDto> items = [];
        List<ApiError> errors = [];

        foreach (var hostname in hostnames)
        {
            if (_errorResponses.TryGetValue(hostname, out var error))
            {
                errors.Add(error.Error);
                continue;
            }

            if (_responses.TryGetValue(hostname, out var dto))
            {
                items.Add(dto);
                continue;
            }

            if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
            {
                errors.Add(new ApiError(_defaultErrorCode, _defaultErrorMessage));
                continue;
            }

            items.Add(GeoLocationDtoFactory.CreateGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country"));
        }

        var collection = new CollectionModel<GeoLocationDto> { Items = items };
        var response = new ApiResponse<CollectionModel<GeoLocationDto>>(collection)
        {
            Errors = errors.Count > 0 ? [.. errors] : null
        };
        return Task.FromResult(new ApiResult<CollectionModel<GeoLocationDto>>(HttpStatusCode.OK, response));
    }

    public Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default)
    {
        _deletedAddresses.Add(hostname);
        return Task.FromResult(new ApiResult(HttpStatusCode.OK, new ApiResponse()));
    }
}
