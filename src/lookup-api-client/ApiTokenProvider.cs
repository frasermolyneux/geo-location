using Azure.Core;
using Azure.Identity;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MX.GeoLocation.GeoLocationApi.Client;

public class ApiTokenProvider : IApiTokenProvider
{
    private readonly ILogger<ApiTokenProvider> logger;
    private readonly IMemoryCache memoryCache;
    private readonly IConfiguration configuration;

    public ApiTokenProvider(
        ILogger<ApiTokenProvider> logger,
        IMemoryCache memoryCache,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.memoryCache = memoryCache;
        this.configuration = configuration;
    }

    private string ApiApplicationAudience => configuration["geolocation-api-application-audience"];

    public async Task<string> GetAccessToken()
    {
        if (memoryCache.TryGetValue("servers-api-access-token", out AccessToken accessToken))
        {
            if (DateTime.UtcNow < accessToken.ExpiresOn)
                return accessToken.Token;
        }

        var tokenCredential = new DefaultAzureCredential();

        try
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext(new[] { $"{ApiApplicationAudience}" }));
            memoryCache.Set("geolocation-api-access-token", accessToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to get identity token from AAD for audience: '{ApiApplicationAudience}'");
            throw;
        }

        return accessToken.Token;
    }
}