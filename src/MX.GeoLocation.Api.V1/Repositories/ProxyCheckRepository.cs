using System.Text.Json;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class ProxyCheckRepository : IProxyCheckRepository
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ProxyCheckRepository> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public ProxyCheckRepository(
            IHttpClientFactory httpClientFactory,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            ILogger<ProxyCheckRepository> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _baseUrl = (configuration["ProxyCheck:BaseUrl"] ?? "https://proxycheck.io/v2/").TrimEnd('/') + "/";
            _apiKey = configuration["ProxyCheck:ApiKey"]
                ?? throw new InvalidOperationException("ProxyCheck:ApiKey is not configured. Add this secret to Key Vault.");
        }

        public async Task<ProxyCheckDto> GetProxyCheckData(string address, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            var operation = _telemetryClient.StartOperation<DependencyTelemetry>("ProxyCheckQuery");
            operation.Telemetry.Type = "HTTP";
            operation.Telemetry.Target = "proxycheck.io";
            operation.Telemetry.Data = address;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ProxyCheck");
                var apiUrl = $"{_baseUrl}{address}?key={_apiKey}&vpn=1&asn=1&risk=1&seen=1&tag=geolocation";

                var response = await httpClient.GetAsync(apiUrl, cancellationToken).ConfigureAwait(false);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("ProxyCheck API error for {Address}: {StatusCode} - {Response}",
                        address, response.StatusCode, responseContent);
                    throw new HttpRequestException($"ProxyCheck API returned {response.StatusCode}");
                }

                var result = ParseResponse(address, responseContent);

                operation.Telemetry.Success = true;
                operation.Telemetry.ResultCode = "200";
                return result;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                operation.Telemetry.Success = false;
                operation.Telemetry.ResultCode = ex.Message;
                _telemetryClient.TrackException(ex);
                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operation);
            }
        }

        private ProxyCheckDto ParseResponse(string address, string responseContent)
        {
            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("status", out var statusElement) || statusElement.GetString() != "ok")
            {
                var errorMsg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Unknown error";
                throw new InvalidOperationException($"ProxyCheck returned non-ok status: {errorMsg}");
            }

            if (!root.TryGetProperty(address, out var ipElement))
                throw new InvalidOperationException($"ProxyCheck response did not contain data for {address}");

            return new ProxyCheckDto
            {
                Address = address,
                TranslatedAddress = address,
                RiskScore = ipElement.TryGetProperty("risk", out var riskEl) && riskEl.TryGetInt32(out var risk) ? risk : 0,
                IsProxy = ipElement.TryGetProperty("proxy", out var proxyEl) && proxyEl.GetString() == "yes",
                IsVpn = ipElement.TryGetProperty("type", out var typeEl) && string.Equals(typeEl.GetString(), "vpn", StringComparison.OrdinalIgnoreCase),
                ProxyType = ipElement.TryGetProperty("type", out var typeVal) ? typeVal.GetString() ?? string.Empty : string.Empty,
                Country = ipElement.TryGetProperty("country", out var countryEl) ? countryEl.GetString() ?? string.Empty : string.Empty,
                Region = ipElement.TryGetProperty("region", out var regionEl) ? regionEl.GetString() ?? string.Empty : string.Empty,
                AsNumber = ipElement.TryGetProperty("asn", out var asnEl) ? asnEl.GetString() ?? string.Empty : string.Empty,
                AsOrganization = ipElement.TryGetProperty("provider", out var providerEl) ? providerEl.GetString() ?? string.Empty : string.Empty
            };
        }
    }
}
