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
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _cityErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _insightsErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentBag<string> _cityLookedUpAddresses = [];
    private readonly ConcurrentBag<string> _insightsLookedUpAddresses = [];
    private DefaultLookupBehavior _defaultBehavior = DefaultLookupBehavior.ReturnGenericSuccess;
    private HttpStatusCode _defaultErrorStatusCode = HttpStatusCode.NotFound;
    private string _defaultErrorCode = "NOT_FOUND";
    private string _defaultErrorMessage = "Address not configured in fake";

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

    /// <summary>
    /// Registers a canned error response for city lookups of a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddCityErrorResponse(string address, HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _cityErrorResponses[address] = (statusCode, new ApiError(errorCode, errorMessage));
        return this;
    }

    /// <summary>
    /// Registers a canned error response for insights lookups of a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddInsightsErrorResponse(string address, HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _insightsErrorResponses[address] = (statusCode, new ApiError(errorCode, errorMessage));
        return this;
    }

    /// <summary>
    /// Configures how unconfigured addresses are handled.
    /// When set to <see cref="DefaultLookupBehavior.ReturnError"/>, lookups for addresses
    /// without a canned response will return an error instead of a generic success.
    /// </summary>
    public FakeGeoLookupApiV1_1 SetDefaultBehavior(
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
    public FakeGeoLookupApiV1_1 Reset()
    {
        _cityResponses.Clear();
        _insightsResponses.Clear();
        _cityErrorResponses.Clear();
        _insightsErrorResponses.Clear();
        while (_cityLookedUpAddresses.TryTake(out _)) { }
        while (_insightsLookedUpAddresses.TryTake(out _)) { }
        _defaultBehavior = DefaultLookupBehavior.ReturnGenericSuccess;
        _defaultErrorStatusCode = HttpStatusCode.NotFound;
        _defaultErrorCode = "NOT_FOUND";
        _defaultErrorMessage = "Address not configured in fake";
        return this;
    }

    /// <summary>
    /// Returns the set of addresses looked up via <see cref="GetCityGeoLocation"/>.
    /// </summary>
    public IReadOnlyCollection<string> CityLookedUpAddresses => _cityLookedUpAddresses.ToArray();

    /// <summary>
    /// Returns the set of addresses looked up via <see cref="GetInsightsGeoLocation"/>.
    /// </summary>
    public IReadOnlyCollection<string> InsightsLookedUpAddresses => _insightsLookedUpAddresses.ToArray();

    public Task<ApiResult<CityGeoLocationDto>> GetCityGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        _cityLookedUpAddresses.Add(hostname);

        if (_cityErrorResponses.TryGetValue(hostname, out var error))
        {
            return Task.FromResult(new ApiResult<CityGeoLocationDto>(error.StatusCode,
                new ApiResponse<CityGeoLocationDto>(error.Error)));
        }

        if (_cityResponses.TryGetValue(hostname, out var dto))
        {
            return Task.FromResult(new ApiResult<CityGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<CityGeoLocationDto>(dto)));
        }

        if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
        {
            return Task.FromResult(new ApiResult<CityGeoLocationDto>(_defaultErrorStatusCode,
                new ApiResponse<CityGeoLocationDto>(new ApiError(_defaultErrorCode, _defaultErrorMessage))));
        }

        dto = GeoLocationDtoFactory.CreateCityGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");
        return Task.FromResult(new ApiResult<CityGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<CityGeoLocationDto>(dto)));
    }

    public Task<ApiResult<InsightsGeoLocationDto>> GetInsightsGeoLocation(string hostname, CancellationToken cancellationToken = default)
    {
        _insightsLookedUpAddresses.Add(hostname);

        if (_insightsErrorResponses.TryGetValue(hostname, out var error))
        {
            return Task.FromResult(new ApiResult<InsightsGeoLocationDto>(error.StatusCode,
                new ApiResponse<InsightsGeoLocationDto>(error.Error)));
        }

        if (_insightsResponses.TryGetValue(hostname, out var dto))
        {
            return Task.FromResult(new ApiResult<InsightsGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<InsightsGeoLocationDto>(dto)));
        }

        if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
        {
            return Task.FromResult(new ApiResult<InsightsGeoLocationDto>(_defaultErrorStatusCode,
                new ApiResponse<InsightsGeoLocationDto>(new ApiError(_defaultErrorCode, _defaultErrorMessage))));
        }

        dto = GeoLocationDtoFactory.CreateInsightsGeoLocation(address: hostname, cityName: "Test City", countryName: "Test Country");
        return Task.FromResult(new ApiResult<InsightsGeoLocationDto>(HttpStatusCode.OK, new ApiResponse<InsightsGeoLocationDto>(dto)));
    }
}
