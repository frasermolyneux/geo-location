using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.LookupWebApi.Repositories
{
    public class MaxMindGeoLocationRepository : IMaxMindGeoLocationRepository
    {
        private readonly IConfiguration configuration;
        private readonly TelemetryClient telemetryClient;

        public MaxMindGeoLocationRepository(
            IConfiguration configuration,
            TelemetryClient telemetryClient)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        public async Task<GeoLocationDto> GetGeoLocation(string address, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            using var reader = CreateClient();
            var operation = StartOperation("MaxMindCityQuery", address);

            try
            {
                var lookupResult = await reader.CityAsync(address);

#pragma warning disable CS0618 // Deprecated Traits properties — v1 API uses string dictionary for backward compatibility
                var traits = new Dictionary<string, string?>
                {
                    {"AutonomousSystemNumber", lookupResult.Traits.AutonomousSystemNumber?.ToString()},
                    {"AutonomousSystemOrganization", lookupResult.Traits?.AutonomousSystemOrganization},
                    {"ConnectionType", lookupResult.Traits?.ConnectionType},
                    {"Domain", lookupResult.Traits?.Domain},
                    {"IPAddress", lookupResult.Traits?.IPAddress},
                    {"IsAnonymous", lookupResult.Traits?.IsAnonymous.ToString()},
                    {"IsAnonymousVpn", lookupResult.Traits?.IsAnonymousVpn.ToString()},
                    {"IsHostingProvider", lookupResult.Traits?.IsHostingProvider.ToString()},
                    {"IsLegitimateProxy", lookupResult.Traits?.IsLegitimateProxy.ToString()},
                    {"IsPublicProxy", lookupResult.Traits?.IsPublicProxy.ToString()},
                    {"IsTorExitNode", lookupResult.Traits?.IsTorExitNode.ToString()},
                    {"Isp", lookupResult.Traits?.Isp},
                    {"Organization", lookupResult.Traits?.Organization},
                    {"StaticIPScore", lookupResult.Traits?.StaticIPScore.ToString()},
                    {"UserCount", lookupResult.Traits?.UserCount.ToString()},
                    {"UserType", lookupResult.Traits?.UserType}
                };
#pragma warning restore CS0618

                var result = new GeoLocationDto()
                {
                    Address = address,
                    TranslatedAddress = address,
                    ContinentCode = lookupResult.Continent?.Code ?? string.Empty,
                    ContinentName = lookupResult.Continent?.Name ?? string.Empty,
                    CountryCode = lookupResult.Country?.IsoCode ?? string.Empty,
                    CountryName = lookupResult.Country?.Name ?? string.Empty,
                    IsEuropeanUnion = lookupResult.Country?.IsInEuropeanUnion ?? false,
                    CityName = lookupResult.City?.Name ?? string.Empty,
                    PostalCode = lookupResult.Postal?.Code ?? string.Empty,
                    RegisteredCountry = lookupResult.RegisteredCountry?.IsoCode ?? string.Empty,
                    RepresentedCountry = lookupResult.RepresentedCountry?.IsoCode ?? string.Empty,
                    Latitude = lookupResult.Location?.Latitude ?? 0.0,
                    Longitude = lookupResult.Location?.Longitude ?? 0.0,
                    AccuracyRadius = lookupResult.Location?.AccuracyRadius ?? 0,
                    Timezone = lookupResult.Location?.TimeZone ?? string.Empty,
                    Traits = traits
                };

                MarkSuccess(operation);
                return result;
            }
            catch (GeoIP2Exception ex)
            {
                HandleException(operation, ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        public async Task<CityGeoLocationDto> GetCityGeoLocation(string address, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            using var reader = CreateClient();
            var operation = StartOperation("MaxMindCityQuery", address);

            try
            {
                var lookupResult = await reader.CityAsync(address);
                var result = MapToCityDto(address, lookupResult);
                MarkSuccess(operation);
                return result;
            }
            catch (GeoIP2Exception ex)
            {
                HandleException(operation, ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        public async Task<InsightsGeoLocationDto> GetInsightsGeoLocation(string address, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(address);

            using var reader = CreateClient();
            var operation = StartOperation("MaxMindInsightsQuery", address);

            try
            {
                var lookupResult = await reader.InsightsAsync(address);
                var anonymizer = lookupResult.Anonymizer;

                var dto = MapToInsightsDto(address, lookupResult);
                dto.Anonymizer = new AnonymizerDto
                {
                    Confidence = anonymizer?.Confidence,
                    IsAnonymous = anonymizer?.IsAnonymous ?? false,
                    IsAnonymousVpn = anonymizer?.IsAnonymousVpn ?? false,
                    IsHostingProvider = anonymizer?.IsHostingProvider ?? false,
                    IsPublicProxy = anonymizer?.IsPublicProxy ?? false,
                    IsResidentialProxy = anonymizer?.IsResidentialProxy ?? false,
                    IsTorExitNode = anonymizer?.IsTorExitNode ?? false,
                    NetworkLastSeen = anonymizer?.NetworkLastSeen?.ToString("yyyy-MM-dd"),
                    ProviderName = anonymizer?.ProviderName
                };

                MarkSuccess(operation);
                return dto;
            }
            catch (GeoIP2Exception ex)
            {
                HandleException(operation, ex);
                throw;
            }
            finally
            {
                telemetryClient.StopOperation(operation);
            }
        }

        private static CityGeoLocationDto MapToCityDto(string address, MaxMind.GeoIP2.Responses.AbstractCityResponse lookupResult)
        {
            return PopulateCityFields(new CityGeoLocationDto(), address, lookupResult);
        }

        private static InsightsGeoLocationDto MapToInsightsDto(string address, MaxMind.GeoIP2.Responses.AbstractCityResponse lookupResult)
        {
            return PopulateCityFields(new InsightsGeoLocationDto(), address, lookupResult);
        }

        private static T PopulateCityFields<T>(T dto, string address, MaxMind.GeoIP2.Responses.AbstractCityResponse lookupResult) where T : CityGeoLocationDto
        {
            dto.Address = address;
            dto.TranslatedAddress = address;
            dto.ContinentCode = lookupResult.Continent?.Code ?? string.Empty;
            dto.ContinentName = lookupResult.Continent?.Name ?? string.Empty;
            dto.CountryCode = lookupResult.Country?.IsoCode ?? string.Empty;
            dto.CountryName = lookupResult.Country?.Name ?? string.Empty;
            dto.IsEuropeanUnion = lookupResult.Country?.IsInEuropeanUnion ?? false;
            dto.CityName = lookupResult.City?.Name ?? string.Empty;
            dto.PostalCode = lookupResult.Postal?.Code ?? string.Empty;
            dto.RegisteredCountry = lookupResult.RegisteredCountry?.IsoCode ?? string.Empty;
            dto.RepresentedCountry = lookupResult.RepresentedCountry?.IsoCode ?? string.Empty;
            dto.Latitude = lookupResult.Location?.Latitude ?? 0.0;
            dto.Longitude = lookupResult.Location?.Longitude ?? 0.0;
            dto.AccuracyRadius = lookupResult.Location?.AccuracyRadius ?? 0;
            dto.Timezone = lookupResult.Location?.TimeZone ?? string.Empty;
            dto.Subdivisions = lookupResult.Subdivisions?
                .Select(s => s.Name ?? string.Empty)
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList() ?? [];
            dto.NetworkTraits = MapNetworkTraits(lookupResult.Traits);
            return dto;
        }

        private static NetworkTraitsDto MapNetworkTraits(MaxMind.GeoIP2.Model.Traits? traits)
        {
            return new NetworkTraitsDto
            {
                AutonomousSystemNumber = traits?.AutonomousSystemNumber,
                AutonomousSystemOrganization = traits?.AutonomousSystemOrganization,
                ConnectionType = traits?.ConnectionType,
                Domain = traits?.Domain,
                IPAddress = traits?.IPAddress,
                IsAnycast = traits?.IsAnycast ?? false,
                Isp = traits?.Isp,
                MobileCountryCode = traits?.MobileCountryCode,
                MobileNetworkCode = traits?.MobileNetworkCode,
                Network = traits?.Network?.ToString(),
                Organization = traits?.Organization,
                StaticIPScore = traits?.StaticIPScore,
                UserCount = traits?.UserCount,
                UserType = traits?.UserType
            };
        }

        private WebServiceClient CreateClient()
        {
            var userIdString = configuration["maxmind_userid"];
            if (!int.TryParse(userIdString, out var userId))
                throw new InvalidOperationException($"The 'maxmind_userid' configuration value '{userIdString}' is not a valid integer.");

            var licenseKey = configuration["maxmind_apikey"] ?? throw new InvalidOperationException("The 'maxmind_apikey' configuration value is not set.");
            return new WebServiceClient(userId, licenseKey);
        }

        private IOperationHolder<DependencyTelemetry> StartOperation(string operationName, string address)
        {
            var operation = telemetryClient.StartOperation<DependencyTelemetry>(operationName);
            operation.Telemetry.Type = "HTTP";
            operation.Telemetry.Target = "geoip.maxmind.com";
            operation.Telemetry.Data = address;
            return operation;
        }

        private static void MarkSuccess(IOperationHolder<DependencyTelemetry> operation)
        {
            operation.Telemetry.Success = true;
            operation.Telemetry.ResultCode = "200";
        }

        private void HandleException(IOperationHolder<DependencyTelemetry> operation, Exception ex)
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResultCode = ex.Message;
            telemetryClient.TrackException(ex);
        }
    }
}
