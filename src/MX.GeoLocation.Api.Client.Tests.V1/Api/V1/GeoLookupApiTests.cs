using System.Net;

using Microsoft.Extensions.Logging;

using MX.Api.Client;
using MX.Api.Client.Auth;
using MX.GeoLocation.Api.Client.V1;

using RestSharp;

namespace MX.GeoLocation.Api.Client.Tests.V1
{
    internal class GeoLookupApiTests
    {
        private ILogger<BaseApi<GeoLocationApiClientOptions>> fakeLogger = null!;
        private IApiTokenProvider fakeApiTokenProvider = null!;
        private IRestClientService fakeRestClientService = null!;

        private GeoLookupApi geoLookupApi = null!;

        private GeoLocationApiClientOptions validGeoLocationApiClientOptions => new GeoLocationApiClientOptions()
        {
            BaseUrl = "https://google.co.uk"
        };

        [SetUp]
        public void SetUp()
        {
            fakeLogger = A.Fake<ILogger<BaseApi<GeoLocationApiClientOptions>>>();
            fakeApiTokenProvider = A.Fake<IApiTokenProvider>();
            fakeRestClientService = A.Fake<IRestClientService>();

            geoLookupApi = new GeoLookupApi(fakeLogger, fakeApiTokenProvider, fakeRestClientService, validGeoLocationApiClientOptions);
        }

        [Test]
        public async Task GetGeoLocationShouldPassThroughApiResponse()
        {
            // Arrange
            var jsonPayload = "{\r\n  \"data\": {\r\n    \"address\": \"google.co.uk\",\r\n    \"translatedAddress\": \"142.250.187.195\",\r\n    \"continentCode\": \"NA\",\r\n    \"continentName\": \"North America\",\r\n    \"countryCode\": \"US\",\r\n    \"countryName\": \"United States\",\r\n    \"isEuropeanUnion\": false,\r\n    \"cityName\": \"\",\r\n    \"postalCode\": \"\",\r\n    \"registeredCountry\": \"US\",\r\n    \"representedCountry\": \"\",\r\n    \"latitude\": 37.751,\r\n    \"longitude\": -97.822,\r\n    \"accuracyRadius\": 1000,\r\n    \"timezone\": \"America/Chicago\",\r\n    \"traits\": {\r\n      \"AutonomousSystemNumber\": \"15169\",\r\n      \"AutonomousSystemOrganization\": \"GOOGLE\",\r\n      \"ConnectionType\": null,\r\n      \"Domain\": \"1e100.net\",\r\n      \"IPAddress\": \"142.250.187.195\",\r\n      \"IsAnonymous\": \"False\",\r\n      \"IsAnonymousVpn\": \"False\",\r\n      \"IsHostingProvider\": \"False\",\r\n      \"IsLegitimateProxy\": \"False\",\r\n      \"IsPublicProxy\": \"False\",\r\n      \"IsTorExitNode\": \"False\",\r\n      \"Isp\": \"Google Servers\",\r\n      \"Organization\": \"Google Servers\",\r\n      \"StaticIPScore\": \"\",\r\n      \"UserCount\": \"\",\r\n      \"UserType\": null\r\n    }\r\n  },\r\n  \"statusCode\": 200,\r\n  \"errors\": []\r\n}";

            RestResponse restResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonPayload
            };

            A.CallTo(() => fakeRestClientService.ExecuteAsync(A<string>.Ignored, A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            // Act
            var result = await geoLookupApi.GetGeoLocation("google.co.uk");

            // Assert
            result.Should().NotBeNull();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Result?.Data?.Address.Should().Be("google.co.uk");
        }


        [Test]
        public async Task GetGeoLocationsShouldPassThroughApiResponse()
        {
            // Arrange
            var jsonPayload = "{\"data\":{\"totalRecords\":3,\"filteredRecords\":3,\"items\":[{\"address\":\"13.64.69.151\",\"translatedAddress\":\"13.64.69.151\",\"continentCode\":\"NA\",\"continentName\":\"North America\",\"countryCode\":\"US\",\"countryName\":\"United States\",\"isEuropeanUnion\":false,\"cityName\":\"San Jose\",\"postalCode\":\"95141\",\"registeredCountry\":\"US\",\"representedCountry\":null,\"latitude\":37.1835,\"longitude\":-121.7714,\"accuracyRadius\":20,\"timezone\":\"America/Los_Angeles\",\"traits\":{\"AutonomousSystemNumber\":\"8075\",\"AutonomousSystemOrganization\":\"MICROSOFT-CORP-MSN-AS-BLOCK\",\"ConnectionType\":null,\"Domain\":null,\"IPAddress\":\"13.64.69.151\",\"IsAnonymous\":\"False\",\"IsAnonymousVpn\":\"False\",\"IsHostingProvider\":\"False\",\"IsLegitimateProxy\":\"False\",\"IsPublicProxy\":\"False\",\"IsTorExitNode\":\"False\",\"Isp\":\"Microsoft Azure\",\"Organization\":\"Microsoft Azure\",\"StaticIPScore\":\"\",\"UserCount\":\"\",\"UserType\":null}},{\"address\":\"2603:1040:1302::580\",\"translatedAddress\":\"2603:1040:1302::580\",\"continentCode\":\"AS\",\"continentName\":\"Asia\",\"countryCode\":\"TW\",\"countryName\":\"Taiwan\",\"isEuropeanUnion\":false,\"cityName\":\"Taipei\",\"postalCode\":\"\",\"registeredCountry\":\"US\",\"representedCountry\":null,\"latitude\":25.0504,\"longitude\":121.5324,\"accuracyRadius\":100,\"timezone\":\"Asia/Taipei\",\"traits\":{\"AutonomousSystemNumber\":\"8075\",\"AutonomousSystemOrganization\":\"MICROSOFT-CORP-MSN-AS-BLOCK\",\"ConnectionType\":null,\"Domain\":null,\"IPAddress\":\"2603:1040:1302::580\",\"IsAnonymous\":\"False\",\"IsAnonymousVpn\":\"False\",\"IsHostingProvider\":\"False\",\"IsLegitimateProxy\":\"False\",\"IsPublicProxy\":\"False\",\"IsTorExitNode\":\"False\",\"Isp\":\"Microsoft Corporation\",\"Organization\":\"Microsoft Corporation\",\"StaticIPScore\":\"\",\"UserCount\":\"\",\"UserType\":null}},{\"address\":\"google.co.uk\",\"translatedAddress\":\"142.250.200.35\",\"continentCode\":\"NA\",\"continentName\":\"North America\",\"countryCode\":\"US\",\"countryName\":\"United States\",\"isEuropeanUnion\":false,\"cityName\":\"\",\"postalCode\":\"\",\"registeredCountry\":\"US\",\"representedCountry\":\"\",\"latitude\":37.751,\"longitude\":-97.822,\"accuracyRadius\":1000,\"timezone\":\"America/Chicago\",\"traits\":{\"AutonomousSystemNumber\":\"15169\",\"AutonomousSystemOrganization\":\"GOOGLE\",\"ConnectionType\":null,\"Domain\":\"1e100.net\",\"IPAddress\":\"142.250.200.35\",\"IsAnonymous\":\"False\",\"IsAnonymousVpn\":\"False\",\"IsHostingProvider\":\"False\",\"IsLegitimateProxy\":\"False\",\"IsPublicProxy\":\"False\",\"IsTorExitNode\":\"False\",\"Isp\":\"Google Servers\",\"Organization\":\"Google Servers\",\"StaticIPScore\":\"\",\"UserCount\":\"\",\"UserType\":null}}]},\"statusCode\":200,\"errors\":[]}";

            RestResponse restResponse = new()
            {
                StatusCode = HttpStatusCode.OK,
                Content = jsonPayload
            };

            A.CallTo(() => fakeRestClientService.ExecuteAsync(A<string>.Ignored, A<RestRequest>.Ignored, default(CancellationToken)))
                .Returns(Task.FromResult(restResponse));

            // Act
            var result = await geoLookupApi.GetGeoLocations(new List<string> { "13.64.69.151", "2603:1040:1302::580", "google.co.uk" });

            // Assert
            result.Should().NotBeNull();

            result.StatusCode.Should().Be(HttpStatusCode.OK);
            result.Result?.Data?.Items.Should().HaveCount(3);
        }

        [TearDown]
        public void TearDown()
        {
            fakeRestClientService?.Dispose();
        }
    }
}
