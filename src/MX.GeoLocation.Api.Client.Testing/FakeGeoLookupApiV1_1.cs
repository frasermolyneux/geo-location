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
    private readonly ConcurrentDictionary<string, ProxyCheckDto> _proxyCheckResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IpIntelligenceDto> _intelligenceResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _cityErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _insightsErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _proxyCheckErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, (HttpStatusCode StatusCode, ApiError Error)> _intelligenceErrorResponses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentBag<string> _cityLookedUpAddresses = [];
    private readonly ConcurrentBag<string> _insightsLookedUpAddresses = [];
    private readonly ConcurrentBag<string> _proxyCheckLookedUpAddresses = [];
    private readonly ConcurrentBag<string> _intelligenceLookedUpAddresses = [];
    private readonly ConcurrentBag<string> _deletedAddresses = [];
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
    /// Registers a canned proxycheck response for a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddProxyCheckResponse(string address, ProxyCheckDto dto)
    {
        _proxyCheckResponses[address] = dto;
        return this;
    }

    /// <summary>
    /// Registers a canned error response for proxycheck lookups of a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddProxyCheckErrorResponse(string address, HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _proxyCheckErrorResponses[address] = (statusCode, new ApiError(errorCode, errorMessage));
        return this;
    }

    /// <summary>
    /// Registers a canned intelligence response for a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddIntelligenceResponse(string address, IpIntelligenceDto dto)
    {
        _intelligenceResponses[address] = dto;
        return this;
    }

    /// <summary>
    /// Registers a canned error response for intelligence lookups of a specific address.
    /// </summary>
    public FakeGeoLookupApiV1_1 AddIntelligenceErrorResponse(string address, HttpStatusCode statusCode, string errorCode, string errorMessage)
    {
        _intelligenceErrorResponses[address] = (statusCode, new ApiError(errorCode, errorMessage));
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
        _proxyCheckResponses.Clear();
        _intelligenceResponses.Clear();
        _cityErrorResponses.Clear();
        _insightsErrorResponses.Clear();
        _proxyCheckErrorResponses.Clear();
        _intelligenceErrorResponses.Clear();
        while (_cityLookedUpAddresses.TryTake(out _)) { }
        while (_insightsLookedUpAddresses.TryTake(out _)) { }
        while (_proxyCheckLookedUpAddresses.TryTake(out _)) { }
        while (_intelligenceLookedUpAddresses.TryTake(out _)) { }
        while (_deletedAddresses.TryTake(out _)) { }
        _defaultBehavior = DefaultLookupBehavior.ReturnGenericSuccess;
        _defaultErrorStatusCode = HttpStatusCode.NotFound;
        _defaultErrorCode = "NOT_FOUND";
        _defaultErrorMessage = "Address not configured in fake";
        return this;
    }

    /// <summary>Returns the set of addresses looked up via <see cref="GetCityGeoLocation"/>.</summary>
    public IReadOnlyCollection<string> CityLookedUpAddresses => _cityLookedUpAddresses.ToArray();

    /// <summary>Returns the set of addresses looked up via <see cref="GetInsightsGeoLocation"/>.</summary>
    public IReadOnlyCollection<string> InsightsLookedUpAddresses => _insightsLookedUpAddresses.ToArray();

    /// <summary>Returns the set of addresses looked up via <see cref="GetProxyCheck"/>.</summary>
    public IReadOnlyCollection<string> ProxyCheckLookedUpAddresses => _proxyCheckLookedUpAddresses.ToArray();

    /// <summary>Returns the set of addresses looked up via <see cref="GetIpIntelligence"/>.</summary>
    public IReadOnlyCollection<string> IntelligenceLookedUpAddresses => _intelligenceLookedUpAddresses.ToArray();

    /// <summary>Returns the set of addresses deleted via <see cref="DeleteMetadata"/>.</summary>
    public IReadOnlyCollection<string> DeletedAddresses => _deletedAddresses.ToArray();

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

    public Task<ApiResult<ProxyCheckDto>> GetProxyCheck(string hostname, CancellationToken cancellationToken = default)
    {
        _proxyCheckLookedUpAddresses.Add(hostname);

        if (_proxyCheckErrorResponses.TryGetValue(hostname, out var error))
            return Task.FromResult(new ApiResult<ProxyCheckDto>(error.StatusCode, new ApiResponse<ProxyCheckDto>(error.Error)));

        if (_proxyCheckResponses.TryGetValue(hostname, out var dto))
            return Task.FromResult(new ApiResult<ProxyCheckDto>(HttpStatusCode.OK, new ApiResponse<ProxyCheckDto>(dto)));

        if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
            return Task.FromResult(new ApiResult<ProxyCheckDto>(_defaultErrorStatusCode, new ApiResponse<ProxyCheckDto>(new ApiError(_defaultErrorCode, _defaultErrorMessage))));

        dto = GeoLocationDtoFactory.CreateProxyCheck(address: hostname);
        return Task.FromResult(new ApiResult<ProxyCheckDto>(HttpStatusCode.OK, new ApiResponse<ProxyCheckDto>(dto)));
    }

    public Task<ApiResult<IpIntelligenceDto>> GetIpIntelligence(string hostname, CancellationToken cancellationToken = default)
    {
        _intelligenceLookedUpAddresses.Add(hostname);

        if (_intelligenceErrorResponses.TryGetValue(hostname, out var error))
            return Task.FromResult(new ApiResult<IpIntelligenceDto>(error.StatusCode, new ApiResponse<IpIntelligenceDto>(error.Error)));

        if (_intelligenceResponses.TryGetValue(hostname, out var dto))
            return Task.FromResult(new ApiResult<IpIntelligenceDto>(HttpStatusCode.OK, new ApiResponse<IpIntelligenceDto>(dto)));

        if (_defaultBehavior == DefaultLookupBehavior.ReturnError)
            return Task.FromResult(new ApiResult<IpIntelligenceDto>(_defaultErrorStatusCode, new ApiResponse<IpIntelligenceDto>(new ApiError(_defaultErrorCode, _defaultErrorMessage))));

        dto = GeoLocationDtoFactory.CreateIpIntelligence(address: hostname);
        return Task.FromResult(new ApiResult<IpIntelligenceDto>(HttpStatusCode.OK, new ApiResponse<IpIntelligenceDto>(dto)));
    }

    public async Task<ApiResult<CollectionModel<IpIntelligenceDto>>> GetIpIntelligences(List<string> hostnames, CancellationToken cancellationToken = default)
    {
        var results = new List<IpIntelligenceDto>();
        foreach (var hostname in hostnames)
        {
            var result = await GetIpIntelligence(hostname, cancellationToken);
            if (result.IsSuccess && result.Result?.Data is not null)
                results.Add(result.Result.Data);
        }

        var response = new ApiResponse<CollectionModel<IpIntelligenceDto>>(
            new CollectionModel<IpIntelligenceDto> { Items = results });
        return new ApiResult<CollectionModel<IpIntelligenceDto>>(HttpStatusCode.OK, response);
    }

    public Task<ApiResult> DeleteMetadata(string hostname, CancellationToken cancellationToken = default)
    {
        _deletedAddresses.Add(hostname);
        return Task.FromResult(new ApiResult(HttpStatusCode.OK, new ApiResponse()));
    }
}
