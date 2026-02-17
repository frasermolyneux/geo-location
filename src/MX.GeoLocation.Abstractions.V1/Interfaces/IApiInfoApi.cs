using MX.Api.Abstractions;
using MX.GeoLocation.Abstractions.Models;

namespace MX.GeoLocation.Abstractions.Interfaces
{
    /// <summary>
    /// Interface for retrieving API information
    /// </summary>
    public interface IApiInfoApi
    {
        /// <summary>
        /// Gets the API build and version information
        /// </summary>
        Task<ApiResult<ApiInfoDto>> GetApiInfo(CancellationToken cancellationToken = default);
    }
}
