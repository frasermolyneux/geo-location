using System.Net;
using MX.GeoLocation.Abstractions.Models;
using MX.GeoLocation.Abstractions.Models.V1;
using MX.GeoLocation.Abstractions.Models.V1_1;

namespace MX.GeoLocation.Api.Client.Testing;

/// <summary>
/// Factory methods for creating geo-location DTOs in tests.
/// Required because DTO properties use internal setters.
/// </summary>
public static class GeoLocationDtoFactory
{
    /// <summary>
    /// Creates a V1 GeoLocationDto with the specified values.
    /// </summary>
    public static GeoLocationDto CreateGeoLocation(
        string address = "8.8.8.8",
        string? translatedAddress = null,
        string? continentCode = "NA",
        string? continentName = "North America",
        string? countryCode = "US",
        string? countryName = "United States",
        bool isEuropeanUnion = false,
        string? cityName = "Mountain View",
        string? postalCode = "94035",
        string? registeredCountry = null,
        string? representedCountry = null,
        double? latitude = 37.386,
        double? longitude = -122.0838,
        int? accuracyRadius = 1000,
        string? timezone = "America/Los_Angeles",
        Dictionary<string, string?>? traits = null)
    {
        return new GeoLocationDto
        {
            Address = address,
            TranslatedAddress = translatedAddress ?? address,
            ContinentCode = continentCode,
            ContinentName = continentName,
            CountryCode = countryCode,
            CountryName = countryName,
            IsEuropeanUnion = isEuropeanUnion,
            CityName = cityName,
            PostalCode = postalCode,
            RegisteredCountry = registeredCountry,
            RepresentedCountry = representedCountry,
            Latitude = latitude,
            Longitude = longitude,
            AccuracyRadius = accuracyRadius,
            Timezone = timezone,
            Traits = traits ?? new Dictionary<string, string?>()
        };
    }

    /// <summary>
    /// Creates a V1.1 CityGeoLocationDto with the specified values.
    /// </summary>
    public static CityGeoLocationDto CreateCityGeoLocation(
        string address = "8.8.8.8",
        string? translatedAddress = null,
        string? continentCode = "NA",
        string? continentName = "North America",
        string? countryCode = "US",
        string? countryName = "United States",
        bool isEuropeanUnion = false,
        string? cityName = "Mountain View",
        string? postalCode = "94035",
        string? registeredCountry = null,
        string? representedCountry = null,
        double? latitude = 37.386,
        double? longitude = -122.0838,
        int? accuracyRadius = 1000,
        string? timezone = "America/Los_Angeles",
        List<string>? subdivisions = null,
        NetworkTraitsDto? networkTraits = null)
    {
        return new CityGeoLocationDto
        {
            Address = address,
            TranslatedAddress = translatedAddress ?? address,
            ContinentCode = continentCode,
            ContinentName = continentName,
            CountryCode = countryCode,
            CountryName = countryName,
            IsEuropeanUnion = isEuropeanUnion,
            CityName = cityName,
            PostalCode = postalCode,
            RegisteredCountry = registeredCountry,
            RepresentedCountry = representedCountry,
            Latitude = latitude,
            Longitude = longitude,
            AccuracyRadius = accuracyRadius,
            Timezone = timezone,
            Subdivisions = subdivisions ?? [],
            NetworkTraits = networkTraits ?? CreateNetworkTraits()
        };
    }

    /// <summary>
    /// Creates a V1.1 InsightsGeoLocationDto with the specified values.
    /// </summary>
    public static InsightsGeoLocationDto CreateInsightsGeoLocation(
        string address = "8.8.8.8",
        string? translatedAddress = null,
        string? continentCode = "NA",
        string? continentName = "North America",
        string? countryCode = "US",
        string? countryName = "United States",
        bool isEuropeanUnion = false,
        string? cityName = "Mountain View",
        string? postalCode = "94035",
        string? registeredCountry = null,
        string? representedCountry = null,
        double? latitude = 37.386,
        double? longitude = -122.0838,
        int? accuracyRadius = 1000,
        string? timezone = "America/Los_Angeles",
        List<string>? subdivisions = null,
        NetworkTraitsDto? networkTraits = null,
        AnonymizerDto? anonymizer = null)
    {
        return new InsightsGeoLocationDto
        {
            Address = address,
            TranslatedAddress = translatedAddress ?? address,
            ContinentCode = continentCode,
            ContinentName = continentName,
            CountryCode = countryCode,
            CountryName = countryName,
            IsEuropeanUnion = isEuropeanUnion,
            CityName = cityName,
            PostalCode = postalCode,
            RegisteredCountry = registeredCountry,
            RepresentedCountry = representedCountry,
            Latitude = latitude,
            Longitude = longitude,
            AccuracyRadius = accuracyRadius,
            Timezone = timezone,
            Subdivisions = subdivisions ?? [],
            NetworkTraits = networkTraits ?? CreateNetworkTraits(),
            Anonymizer = anonymizer ?? CreateAnonymizer()
        };
    }

    /// <summary>
    /// Creates a NetworkTraitsDto with the specified values.
    /// </summary>
    public static NetworkTraitsDto CreateNetworkTraits(
        long? autonomousSystemNumber = 15169,
        string? autonomousSystemOrganization = "Google LLC",
        string? connectionType = "Corporate",
        string? domain = "google.com",
        string? ipAddress = null,
        bool isAnycast = false,
        string? isp = "Google LLC",
        string? mobileCountryCode = null,
        string? mobileNetworkCode = null,
        string? network = null,
        string? organization = "Google LLC",
        double? staticIPScore = null,
        int? userCount = null,
        string? userType = "business")
    {
        return new NetworkTraitsDto
        {
            AutonomousSystemNumber = autonomousSystemNumber,
            AutonomousSystemOrganization = autonomousSystemOrganization,
            ConnectionType = connectionType,
            Domain = domain,
            IPAddress = ipAddress,
            IsAnycast = isAnycast,
            Isp = isp,
            MobileCountryCode = mobileCountryCode,
            MobileNetworkCode = mobileNetworkCode,
            Network = network,
            Organization = organization,
            StaticIPScore = staticIPScore,
            UserCount = userCount,
            UserType = userType
        };
    }

    /// <summary>
    /// Creates an AnonymizerDto with the specified values.
    /// </summary>
    public static AnonymizerDto CreateAnonymizer(
        int? confidence = null,
        bool isAnonymous = false,
        bool isAnonymousVpn = false,
        bool isHostingProvider = false,
        bool isPublicProxy = false,
        bool isResidentialProxy = false,
        bool isTorExitNode = false,
        string? networkLastSeen = null,
        string? providerName = null)
    {
        return new AnonymizerDto
        {
            Confidence = confidence,
            IsAnonymous = isAnonymous,
            IsAnonymousVpn = isAnonymousVpn,
            IsHostingProvider = isHostingProvider,
            IsPublicProxy = isPublicProxy,
            IsResidentialProxy = isResidentialProxy,
            IsTorExitNode = isTorExitNode,
            NetworkLastSeen = networkLastSeen,
            ProviderName = providerName
        };
    }

    /// <summary>
    /// Creates an ApiInfoDto with the specified values.
    /// </summary>
    public static ApiInfoDto CreateApiInfo(
        string version = "1.0.0",
        string buildVersion = "1.0.0.1",
        string assemblyVersion = "1.0.0.0")
    {
        return new ApiInfoDto
        {
            Version = version,
            BuildVersion = buildVersion,
            AssemblyVersion = assemblyVersion
        };
    }
}
