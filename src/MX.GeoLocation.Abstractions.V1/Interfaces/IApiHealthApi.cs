using MX.Api.Abstractions;

namespace MX.GeoLocation.Abstractions.Interfaces
{
    /// <summary>
    /// Interface for checking API health
    /// </summary>
    public interface IApiHealthApi
    {
        /// <summary>
        /// Gets the API health status
        /// </summary>
        Task<ApiResult> CheckHealth(CancellationToken cancellationToken = default);
    }
}
