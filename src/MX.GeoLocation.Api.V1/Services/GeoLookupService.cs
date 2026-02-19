using System.Net;

using MaxMind.GeoIP2.Exceptions;

using MX.Api.Abstractions;
using MX.Api.Web.Extensions;
using MX.GeoLocation.LookupWebApi.Constants;

namespace MX.GeoLocation.LookupWebApi.Services;

public class GeoLookupService : IGeoLookupService
{
    private readonly ILogger<GeoLookupService> _logger;
    private readonly IHostnameResolver _hostnameResolver;

    public GeoLookupService(ILogger<GeoLookupService> logger, IHostnameResolver hostnameResolver)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostnameResolver = hostnameResolver ?? throw new ArgumentNullException(nameof(hostnameResolver));
    }

    public async Task<ApiResult<T>> ExecuteLookup<T>(string hostname, CancellationToken cancellationToken, Func<string, Task<ApiResult<T>>> lookupFunc) where T : class
    {
        try
        {
            var (success, address) = await _hostnameResolver.ResolveHostname(hostname, cancellationToken);
            if (!success || address is null)
                return new ApiResponse<T>(new ApiError(ErrorCodes.INVALID_HOSTNAME, ErrorMessages.INVALID_HOSTNAME)).ToApiResult(HttpStatusCode.BadRequest);

            if (_hostnameResolver.IsLocalAddress(hostname) || _hostnameResolver.IsPrivateOrReservedAddress(address))
                return new ApiResponse<T>(new ApiError(ErrorCodes.LOCAL_ADDRESS, ErrorMessages.LOCAL_ADDRESS)).ToApiResult(HttpStatusCode.BadRequest);

            return await lookupFunc(address);
        }
        catch (AddressNotFoundException ex)
        {
            _logger.LogWarning(ex, "Address not found for {Hostname}", hostname);
            return new ApiResponse<T>(new ApiError(ErrorCodes.ADDRESS_NOT_FOUND, ErrorMessages.ADDRESS_NOT_FOUND)).ToApiResult(HttpStatusCode.NotFound);
        }
        catch (GeoIP2Exception ex)
        {
            _logger.LogError(ex, "GeoIP2 error for {Hostname}", hostname);
            return new ApiResponse<T>(new ApiError(ErrorCodes.GEOIP_ERROR, ex.Message)).ToApiResult(HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during geolocation lookup for {Hostname}", hostname);
            return new ApiResponse<T>(new ApiError(ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred")).ToApiResult(HttpStatusCode.InternalServerError);
        }
    }
}
