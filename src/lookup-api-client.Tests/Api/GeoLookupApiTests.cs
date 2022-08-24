using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MX.GeoLocation.GeoLocationApi.Client.Api;

using RestSharp;

using System.Net;

namespace MX.GeoLocation.GeoLocationApi.Client.Tests.Api
{
    internal class GeoLookupApiTests
    {
        private ILogger<GeoLookupApi> fakeLogger;
        private IOptions<GeoLocationApiClientOptions> fakeOptions;
        private IApiTokenProvider fakeApiTokenProvider;
        private IRestClientSingleton fakeRestClientSingleton;

        private GeoLookupApi geoLookupApi;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions
        {
            BaseUrl = "https://google.co.uk",
            ApiKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
        };

        [SetUp]
        public void SetUp()
        {
            fakeLogger = A.Fake<ILogger<GeoLookupApi>>();
            fakeOptions = A.Fake<IOptions<GeoLocationApiClientOptions>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientSingleton = A.Fake<IRestClientSingleton>();

            A.CallTo(() => fakeOptions.Value).Returns(validGeoLocationApiClientOptions);
            A.CallTo(() => fakeApiTokenProvider.GetAccessToken()).Returns("mytoken");

            geoLookupApi = new GeoLookupApi(fakeLogger, fakeOptions, fakeApiTokenProvider, fakeRestClientSingleton);
        }

        [Test]
        public async Task GetGeoLocationReturnsApiResponsePassesThrough()
        {
            // Arrange
            var jsonPayload = "{\r\n  \"result\": {\r\n    \"address\": \"google.co.uk\",\r\n    \"translatedAddress\": \"142.250.187.195\",\r\n    \"continentCode\": \"NA\",\r\n    \"continentName\": \"North America\",\r\n    \"countryCode\": \"US\",\r\n    \"countryName\": \"United States\",\r\n    \"isEuropeanUnion\": false,\r\n    \"cityName\": \"\",\r\n    \"postalCode\": \"\",\r\n    \"registeredCountry\": \"US\",\r\n    \"representedCountry\": \"\",\r\n    \"latitude\": 37.751,\r\n    \"longitude\": -97.822,\r\n    \"accuracyRadius\": 1000,\r\n    \"timezone\": \"America/Chicago\",\r\n    \"traits\": {\r\n      \"AutonomousSystemNumber\": \"15169\",\r\n      \"AutonomousSystemOrganization\": \"GOOGLE\",\r\n      \"ConnectionType\": null,\r\n      \"Domain\": \"1e100.net\",\r\n      \"IPAddress\": \"142.250.187.195\",\r\n      \"IsAnonymous\": \"False\",\r\n      \"IsAnonymousVpn\": \"False\",\r\n      \"IsHostingProvider\": \"False\",\r\n      \"IsLegitimateProxy\": \"False\",\r\n      \"IsPublicProxy\": \"False\",\r\n      \"IsTorExitNode\": \"False\",\r\n      \"Isp\": \"Google Servers\",\r\n      \"Organization\": \"Google Servers\",\r\n      \"StaticIPScore\": \"\",\r\n      \"UserCount\": \"\",\r\n      \"UserType\": null\r\n    }\r\n  },\r\n  \"isSuccess\": true,\r\n  \"statusCode\": \"OK\",\r\n  \"errors\": [],\r\n  \"isNotFound\": false\r\n}";

            RestResponse restResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonPayload
            };

            A.CallTo(() => fakeRestClientSingleton.ExecuteAsync(A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            // Act
            var result = await geoLookupApi.GetGeoLocation("google.co.uk");

            // Assert
            result.Should().NotBeNull();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Result.Address.Should().Be("google.co.uk");
        }
    }
}
